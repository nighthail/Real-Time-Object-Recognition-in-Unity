import cv2
import numpy as np
import socket
import mediapipe as mp
import keyboard

# Server configuration
server_ip = '127.0.0.1'
video_server_port = 8001
resolution_port = 8003  # Port to send resolution data
landmark_server_port = 8002  # Port to send landmark data

# Initialize UDP clients
udp_client_video = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp_client_resolution = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp_client_landmark = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)  # New UDP client for landmarks

# MediaPipe hands setup
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils
hands = mp_hands.Hands(static_image_mode=False, max_num_hands=2, min_detection_confidence=0.7)

# Open video capture
cap = cv2.VideoCapture(0)  # Use the appropriate camera index or video file

print("Starting Video feed process. Kill terminal to interrupt")

while True:
    ret, frame = cap.read()
    if not ret:
        break

    # Flip the frame horizontally for natural interaction
    frame = cv2.flip(frame, 1)

    # Resize the frame to a smaller resolution before sending
    frame = cv2.resize(frame, (320, 240))  # Change to smaller resolution if necessary

    # Get camera resolution
    height, width, _ = frame.shape

    # Send the resolution periodically
    resolution_data = f"{width},{height}".encode()
    udp_client_resolution.sendto(resolution_data, (server_ip, resolution_port))

    # Convert the frame to RGB for MediaPipe processing
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    # Process the frame with MediaPipe Hands
    result = hands.process(rgb_frame)

    # Prepare landmark data to send
    landmarks_data = []
    if result.multi_hand_landmarks:
        for hand_landmarks in result.multi_hand_landmarks:
            hand_data = []
            for lm in hand_landmarks.landmark:
                # Append x, y, z coordinates for each landmark (normalized to [0, 1])
                hand_data.append(f"{lm.x},{lm.y},{lm.z}")
            landmarks_data.append(';'.join(hand_data))

    # Send the landmarks data via UDP as a plain text string
    landmark_data_str = '|'.join(landmarks_data).encode('utf-8')
    udp_client_landmark.sendto(landmark_data_str, (server_ip, landmark_server_port))

    # Draw hand landmarks if any are detected
    if result.multi_hand_landmarks:
        for hand_landmarks in result.multi_hand_landmarks:
            mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

    # Encode the frame as JPEG after drawing landmarks
    _, buffer = cv2.imencode('.jpg', frame)
    data = buffer.tobytes()

    # Send video feed in chunks
    chunk_size = 65000
    for i in range(0, len(data), chunk_size):
        chunk = data[i:i + chunk_size]
        udp_client_video.sendto(chunk, (server_ip, video_server_port))

    # Break loop if 'q' is pressed
    if keyboard.is_pressed('q'):
        print("Interrupted process by user")
        break

# Release resources
cap.release()
udp_client_video.close()
udp_client_resolution.close()
udp_client_landmark.close()
