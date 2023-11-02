# JIP Ethereal Matter Unity
This repository is for work done to complete the Joint Interdisciplinary Project. During this project we worked in a team of 3 students in order to help Ethereal Matter further develop their product: the immersive VR fitness experience.

Code within this repository is for the Unity game engine and mostly involves a DTW algorithm that is used to compare motion in real-time. The goal was to basically compare movement in real life and tell the user if they are performing an exercise correctly.

# Usage
The Unity scene is not included in the git because Unity is really annoything with GIT as any small changes in the scene will cause merge conflicts. Despite this it is super easy to integrate this code into your own Unity scene. Just pop the *DTWmodelsquatt.cs* onto the avatar:

![image](https://github.com/CRWijn/JIP-Ethereal-Matter-Unity/assets/107326704/9cb4939a-f18c-4f77-a832-7b6fd8c691be)

There are a couple of check boxes on the script:

![image](https://github.com/CRWijn/JIP-Ethereal-Matter-Unity/assets/107326704/8124764f-605c-4655-9c8b-666ca2000c9d)

The function of each boolean can easily be seen by inspecting the code. Otherwise a short description is provided below.
- **Is Ref** defines whether the avatar is a reference model or not. If it is it won't compute the DTW when the play button is pressed.
- **Write Ref** is used to enable writing data to a file. If enable, on start the files of all defined joints will be cleared. When running the scene press space to begin and stop recording data.
- **Toggle Rec** is used to start recording data as soon as the play button is pressed. This is used when you have an animation playing since it will start as soon as the scene loads after the play button is pressed. This won't do anything if **Write Ref** is set to false.
- **Use Cost Function** was used early on to turn on or off whether different starting points are evaluated using a cost function.
- **Debug Mode** Is used to write certain values to a file for debugging or graphing results.
- **OBEDTW** determines whether the team developed method is used or OBEDTW.

Joints to be tracked can be added via the plus button under **Joints**:

![image](https://github.com/CRWijn/JIP-Ethereal-Matter-Unity/assets/107326704/8b9341ca-a91d-4fec-b323-6e14e1e7ef99)

After adding a joint the element can be expanded to reveal some configurable properties:

![image](https://github.com/CRWijn/JIP-Ethereal-Matter-Unity/assets/107326704/e4cff5d7-a492-47a5-ac4d-aace04e02455)

Give the joint a name and connect transforms that define the angle which the joint creates. By giving **bottom** and **middle** the same transform you can create an angle between **top**, **middle** and the vector (1, 1, 1). This is useful if you only have two joints but still want a specific angle. Here is an example of how the left knee is configured:

![image](https://github.com/CRWijn/JIP-Ethereal-Matter-Unity/assets/107326704/62676393-0122-46ab-9f98-4c5bb7ee087a)

You can add an error margin which is how much the User can differ from the Reference before an event can be activated. This can be adjusted in the **Error Margin** field per joint. In the *bodyAngle.cs*, you can add code for what should happen if the error is larger than the margin in the function *checkFrame()*.

Lastly, back in the *DTWmodelsquatt.cs* script you can configure **Window Size** which says how many frames the script should collect data from the live model before performing the DTW analysis. If Unity is set to run at 60 frames per second then setting the window size to 30 means the DTW will run twice per second.

# Final Comments

It's possible to use the DTW coded in this Unity for other projects. You simply have to change what kind of data is fed into the DTW. Therefore you can also run this on an embedded system with a microcontroller and perform other real-time analysis.

Thank you very much!
