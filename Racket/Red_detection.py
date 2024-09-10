import cv2
import numpy as np

# Initialize the tracker
tracker = cv2.TrackerCSRT_create()

# Initialize tracking status
tracking = False
bbox = None

# Start video capture from webcam
cap = cv2.VideoCapture(0)

while True:
    # Capture frame-by-frame
    ret, frame = cap.read()
    if not ret:
        break

    # If we are tracking, use the tracker to update the position of the ball
    if tracking:
        success, bbox = tracker.update(frame)
        if success:
            # If tracking is successful, draw the bounding box
            p1 = (int(bbox[0]), int(bbox[1]))
            p2 = (int(bbox[0] + bbox[2]), int(bbox[1] + bbox[3]))
            cv2.rectangle(frame, p1, p2, (0, 255, 0), 2)
        else:
            # If tracking fails, stop tracking and fall back to detection
            tracking = False

    # If we are not tracking, perform detection to find the ball
    if not tracking:
        # Convert the frame to HSV color space
        hsv_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

        # Define the range of red color in HSV
        lower_red1 = np.array([0, 120, 70])  # Lower bound for red
        upper_red1 = np.array([10, 255, 255])  # Upper bound for red
        lower_red2 = np.array([170, 120, 70])  # Another lower range for red
        upper_red2 = np.array([180, 255, 255])  # Another upper range for red

        # Create a mask for red color
        mask1 = cv2.inRange(hsv_frame, lower_red1, upper_red1)
        mask2 = cv2.inRange(hsv_frame, lower_red2, upper_red2)

        # Combine the masks
        red_mask = mask1 + mask2

        # Remove small noise using GaussianBlur
        red_mask = cv2.GaussianBlur(red_mask, (5, 5), 0)

        # Find contours in the red mask
        contours, _ = cv2.findContours(red_mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

        # Initialize variables to store the largest ball
        max_radius = 0
        best_circle = None

        # Loop over the contours to find the largest circular one
        for contour in contours:
            # Approximate the contour to check if it's a circle
            approx = cv2.approxPolyDP(contour, 0.02 * cv2.arcLength(contour, True), True)

            # Calculate the circularity of the contour
            area = cv2.contourArea(contour)
            perimeter = cv2.arcLength(contour, True)
            if perimeter == 0:
                continue
            circularity = 4 * np.pi * (area / (perimeter ** 2))

            # Filter contours based on circularity (values close to 1 indicate a circle)
            if 0.7 < circularity < 1.2:
                # Find the minimum enclosing circle
                ((x, y), radius) = cv2.minEnclosingCircle(contour)
                if radius > max_radius and radius > 10:  # Only consider larger circles
                    max_radius = radius
                    best_circle = (int(x), int(y), int(radius))

        # If a valid circle is found, initialize the tracker
        if best_circle is not None:
            x, y, radius = best_circle
            bbox = (x - radius, y - radius, 2 * radius, 2 * radius)  # Define a bounding box around the ball
            tracker.init(frame, bbox)  # Initialize the tracker with the bounding box
            tracking = True  # Switch to tracking mode

    # Show the original frame with the detected or tracked ball
    cv2.imshow('Red Ball Detection and Tracking', frame)

    # Break the loop on 'q' key press
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release the webcam and close windows
cap.release()
cv2.destroyAllWindows()
