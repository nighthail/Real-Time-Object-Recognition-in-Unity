import cv2
import mediapipe as mp

# Initialize MediaPipe Hands and drawing utilities
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils

# Setup webcam feed
cap = cv2.VideoCapture(0)

# Initialize hands detector
with mp_hands.Hands(
    static_image_mode=False,       # Use False to detect hands in a video stream
    max_num_hands=2,               # Maximum number of hands to detect
    min_detection_confidence=0.7,  # Minimum detection confidence
    min_tracking_confidence=0.5    # Minimum tracking confidence
) as hands:

    while cap.isOpened():
        ret, frame = cap.read()  # Capture frame from webcam

        if not ret:
            print("Ignoring empty frame.")
            continue

        # Flip the frame horizontally for a later selfie-view display, and convert the color from BGR to RGB
        frame_rgb = cv2.cvtColor(cv2.flip(frame, 1), cv2.COLOR_BGR2RGB)

        # Process the frame with MediaPipe Hands
        results = hands.process(frame_rgb)

        # Convert back to BGR for OpenCV rendering
        frame_bgr = cv2.cvtColor(frame_rgb, cv2.COLOR_RGB2BGR)

        # Draw hand landmarks if any are detected
        if results.multi_hand_landmarks:
            for hand_landmarks in results.multi_hand_landmarks:
                mp_drawing.draw_landmarks(
                    frame_bgr, hand_landmarks, mp_hands.HAND_CONNECTIONS)

        # Display the resulting frame
        cv2.imshow('Hand Detection', frame_bgr)

        # Break the loop on 'q' key press
        if cv2.waitKey(5) & 0xFF == ord('q'):
            break

# Release the webcam and close windows
cap.release()
cv2.destroyAllWindows()
