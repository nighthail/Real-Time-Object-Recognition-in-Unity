import time
import cv2
import numpy as np
import socket
import keyboard

# Server configuration
server_ip = '127.0.0.1'
video_server_port = 8001
ball_server_port = 8002  # Port to send ball data

# Initialize UDP clients
udp_client_video = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp_client_ball = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)  # New UDP client for ball info

# Initialize the tracker
tracker = cv2.TrackerCSRT_create()

# Initialize tracking status
tracking = False
bbox = None

# Start video capture from webcam
cap = cv2.VideoCapture(0)

# Verify if the webcam is initialized correctly
if not cap.isOpened():
    print("Error: Could not open video feed.")
    exit()

print("Starting Video feed process. Press 'q' to quit, 'r' to reset tracking.")

# Flag to send resolution only once
resolution_sent = False

while True:
    # Capture frame-by-frame
    ret, frame = cap.read()

    if not ret:
        print("Error: Could not read frame.")
        break

    # Resize the frame to a smaller resolution before sending
    frame = cv2.resize(frame, (320, 240))  # Change to smaller resolution if necessary

    # Convert the frame to RGB for processing
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    # If we are tracking, use the tracker to update the position of the ball
    if tracking:
        success, bbox = tracker.update(frame)
        if success:
            # Dynamically update the bounding box as the object moves or changes size
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

        # Step 1: Detect the green ball
        lower_green = np.array([35, 50, 50])  # Lower bound for green
        upper_green = np.array([85, 255, 255])  # Upper bound for green
        green_mask = cv2.inRange(hsv_frame, lower_green, upper_green)

        # Find green contours
        contours, _ = cv2.findContours(green_mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

        max_radius = 0
        best_circle = None

        for contour in contours:
            area = cv2.contourArea(contour)
            perimeter = cv2.arcLength(contour, True)
            if perimeter == 0:
                continue
            circularity = 4 * np.pi * (area / (perimeter ** 2))
            if 0.7 < circularity < 1.2:  # Ensure it's a circular shape
                ((x, y), radius) = cv2.minEnclosingCircle(contour)
                if radius > max_radius and radius > 10:  # Filter based on size
                    max_radius = radius
                    best_circle = (int(x), int(y), int(radius))

        # Step 2: Detect a white circle around the green ball
        if best_circle is not None:
            x, y, radius = best_circle
            bbox = (x - radius, y - radius, 2 * radius, 2 * radius)

            # Now look for white contours (for the circles) in the region around the green ball
            lower_white = np.array([0, 0, 200])  # HSV range for white
            upper_white = np.array([180, 30, 255])

            # Create a mask for white color and search for the white contours
            white_mask = cv2.inRange(hsv_frame, lower_white, upper_white)
            white_contours, _ = cv2.findContours(white_mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

            large_white_circle_found = False
            small_white_circle_found = False

            for white_contour in white_contours:
                (white_x, white_y), white_radius = cv2.minEnclosingCircle(white_contour)
                white_radius = int(white_radius)

                # Check for a large white circle around the green ball
                if radius * 1.2 < white_radius < radius * 1.5:
                    large_white_circle_found = True
                # Check for a small white circle inside the green ball
                if white_radius < radius * 0.5:
                    small_white_circle_found = True

            if large_white_circle_found and small_white_circle_found:
                # Initialize tracking if both circles are found
                tracker.init(frame, bbox)
                tracking = True

                # Draw the dynamically identified object immediately
                cv2.circle(frame, (x, y), int(radius), (0, 255, 0), 2)  # Green circle
                ball_position_data = f"{x},{y}".encode()
                udp_client_ball.sendto(ball_position_data, (server_ip, ball_server_port))
            else:
                # If white circles are not found, reset the tracking attempt
                ball_position_data = f"0,0".encode()
                udp_client_ball.sendto(ball_position_data, (server_ip, ball_server_port))

    # Add delay to avoid overwhelming the network
    time.sleep(0.05)

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
udp_client_ball.close()
