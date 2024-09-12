import time
import cv2
import numpy as np
import socket
import keyboard

# Server configuration
server_ip = '127.0.0.1'
video_server_port = 8001
resolution_port = 8003  # Port to send resolution data
ball_server_port = 8002  # Port to send ball data

# Initialize UDP clients
udp_client_video = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp_client_resolution = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp_client_ball = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)  # New UDP client for ball info

# Initialize the tracker
tracker = cv2.TrackerCSRT_create()

# Initialize tracking status
tracking = False
bbox = None

# Start video capture from webcam
cap = cv2.VideoCapture(0)

print("Starting Video feed process. Press 'q' to quit, 'r' to reset tracking.")

while True:
    # Capture frame-by-frame
    ret, frame = cap.read()
    if not ret:
        break

    # Flip the frame horizontally for natural interaction
    #frame = cv2.flip(frame, 1)

    # Resize the frame to a smaller resolution before sending
    frame = cv2.resize(frame, (320, 240))  # Change to smaller resolution if necessary

    # Get camera resolution
    height, width, _ = frame.shape

    # Send the resolution periodically
    resolution_data = f"{width},{height}".encode()
    udp_client_resolution.sendto(resolution_data, (server_ip, resolution_port))

    # Convert the frame to RGB for processing
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    # If we are tracking, use the tracker to update the position of the ball
    if tracking:
        success, bbox = tracker.update(frame)
        if success:
            # If tracking is successful, draw the bounding box
            p1 = (int(bbox[0]), int(bbox[1]))
            p2 = (int(bbox[0] + bbox[2]), int(bbox[1] + bbox[3]))
            cv2.rectangle(frame, p1, p2, (0, 255, 0), 2)

            # Send ball position (center of the bounding box) to Unity
            x_center = int(bbox[0] + bbox[2] / 2)
            y_center = int(bbox[1] + bbox[3] / 2)
            ball_position_data = f"{x_center},{y_center}".encode()
            udp_client_ball.sendto(ball_position_data, (server_ip, ball_server_port))
        else:
            tracking = False

    # If we are not tracking, perform detection to find the ball
    if not tracking:
        hsv_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

        lower_red1 = np.array([0, 120, 70])
        upper_red1 = np.array([10, 255, 255])
        lower_red2 = np.array([170, 120, 70])
        upper_red2 = np.array([180, 255, 255])

        mask1 = cv2.inRange(hsv_frame, lower_red1, upper_red1)
        mask2 = cv2.inRange(hsv_frame, lower_red2, upper_red2)
        red_mask = mask1 + mask2

        contours, _ = cv2.findContours(red_mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

        max_radius = 0
        best_circle = None

        for contour in contours:
            area = cv2.contourArea(contour)
            perimeter = cv2.arcLength(contour, True)
            if perimeter == 0:
                continue
            circularity = 4 * np.pi * (area / (perimeter ** 2))
            if 0.7 < circularity < 1.2:
                ((x, y), radius) = cv2.minEnclosingCircle(contour)
                if radius > max_radius and radius > 10:
                    max_radius = radius
                    best_circle = (int(x), int(y), int(radius))

        if best_circle is not None:
            x, y, radius = best_circle
            bbox = (x - radius, y - radius, 2 * radius, 2 * radius)
            tracker.init(frame, bbox)
            tracking = True

            # Send ball position to Unity
            ball_position_data = f"{x},{y}".encode()
            udp_client_ball.sendto(ball_position_data, (server_ip, ball_server_port))
        else:
            # If no red ball is found, send (0, 0) or some "no ball" indicator
            ball_position_data = f"0,0".encode()
            udp_client_ball.sendto(ball_position_data, (server_ip, ball_server_port))

    time.sleep(0.05)  # Add delay to avoid overwhelming the network

    # Encode the frame to JPEG
    _, buffer = cv2.imencode('.jpg', frame)
    frame_bytes = buffer.tobytes()

    # Send video feed in chunks
    chunk_size = 65000
    for i in range(0, len(frame_bytes), chunk_size):
        chunk = frame_bytes[i:i + chunk_size]
        udp_client_video.sendto(chunk, (server_ip, video_server_port))

    # Check for key presses
    if keyboard.is_pressed('q'):
        print("Interrupted process by user")
        break

    if keyboard.is_pressed('r'):
        print("Resetting tracking.")
        tracking = False
        bbox = None
        tracker = cv2.TrackerCSRT_create()  # Reinitialize the tracker

# Release the webcam and close resources
cap.release()
cv2.destroyAllWindows()
udp_client_video.close()
udp_client_resolution.close()
udp_client_ball.close()
