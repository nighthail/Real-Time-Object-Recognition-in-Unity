import cv2
import numpy as np
import socket
import mediapipe as mp
import keyboard

# Server configuration
server_ip = '127.0.0.1'
video_server_port = 8001
hand_server_port = 8002

# Initialize UDP client
udp_client_video = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp_client_hand = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# MediaPipe hands setup
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(static_image_mode=False, max_num_hands=1, min_detection_confidence=0.7)

# Open video capture
cap = cv2.VideoCapture(0)  # Use the appropriate camera index or video file

print("Starting Video feed process. Kill terminal to interrupt")

while True:
    ret, frame = cap.read()
    if not ret:
        break

    # Flip the frame horizontally for natural interaction
    frame = cv2.flip(frame, 1)

    # Encode the frame as JPEG
    _, buffer = cv2.imencode('.jpg', frame)
    data = buffer.tobytes()

    # Send video feed in chunks
    chunk_size = 65000  # Adjust chunk size if necessary
    for i in range(0, len(data), chunk_size):
        chunk = data[i:i + chunk_size]
        udp_client_video.sendto(chunk, (server_ip, video_server_port))

    # Convert the frame to RGB and process with MediaPipe
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    result = hands.process(rgb_frame)

    if result.multi_hand_landmarks:
        for hand_landmarks in result.multi_hand_landmarks:
            # Get the position of the index finger tip (landmark 8)
            index_finger_tip = hand_landmarks.landmark[8]
            hand_position = f"{index_finger_tip.x},{index_finger_tip.y}"
            udp_client_hand.sendto(hand_position.encode(), (server_ip, hand_server_port))

    if keyboard.is_pressed('q'):
        print("Interrupted process by user")
        break

# Release resources
cap.release()
udp_client_video.close()
udp_client_hand.close()
