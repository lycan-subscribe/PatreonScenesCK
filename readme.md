# Patreon Scenes Content Kit

There are 2 things so far in this repo:
- A component protocol for getting information to the client, like where the viewer can pose, how they will be animated, which UIs to allow VR raycasting on, etc.
- An editor window for uploading scenes full of these components to the Patreon Scenes asset server, where other people can download them if they are subscribed to your Patreon campaign

# How to Upload Scenes

To use the editor window, just press the button to get a new token, paste the token (with no quotes) into the login box, login, select the minimum tier required to see the scene, and press build and upload with the scene you want to upload open. This will generate files in Assets/_PSCKSceneBundles, which can be deleted if you don't need the builds for yourself or if you're getting errors. Press just build to generate the files but not upload.

# Component Documentation

## ViewerAvatar

This should be on the same object as your Animator, at the root of the mesh/armature you want to use as the viewer's avatar. The Avatar on the Animator should be humanoid so this works properly. Set the viewpoint on the avatar where the viewer's camera will look out of, and make sure it is not blocked off by any polygons.

## ViewerPose

If you put an animation controller into this, it will animate the viewer's avatar with it while they are on the pose. The box collider lets viewers point and click to switch to the pose using their VR controller. Different VR systems will mask out different bones of the animation, e.g. halfbody VR will not animate bones above the hips since the headset and hands can be used instead.

## ViewerUI

Put this on the same object as a Canvas to let viewers use the UI using a VR controller.

## ViewerScene

Use this to select the avatar, poses (optional), and a spawn point (optional) that will show up in your scene.