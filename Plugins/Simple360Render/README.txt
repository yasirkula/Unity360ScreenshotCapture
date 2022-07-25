= 360° Screenshot Capture =

Online documentation & example code available at: https://github.com/yasirkula/Unity360ScreenshotCapture
E-mail: yasirkula@gmail.com

### ABOUT
This plugin helps you capture 360° screenshots in equirectangular format during gameplay.


### HOW TO
Simply call one of the following functions:

public static byte[] I360Render.Capture( int width = 1024, bool encodeAsJPEG = true, Camera renderCam = null, bool faceCameraDirection = true );
public static void CaptureAsync( Action<byte[]> callback, int width = 1024, bool encodeAsJPEG = true, Camera renderCam = null, bool faceCameraDirection = true ); // Unity 2018.2 or later

- width: The width of the resulting image. It must be a power of 2. The height will be equal to width / 2. Be aware that maximum allowed image width is 8192 pixels
- encodeAsJPEG: determines whether the image will be encoded as JPEG or PNG
- renderCam: the camera that will be used to render the 360° image. If set to null, Camera.main will be used
- faceCameraDirection: if set to true, when the 360° image is viewed in a 360° viewer, initial camera rotation will match the rotation of the renderCam. Otherwise, initial camera rotation will be Quaternion.identity (facing Z+ axis)

These functions return a byte[] object either directly or as a callback; you can write these bytes to a file using File.WriteAllBytes.


### FAQ
- Objects are rendered inside out in the 360° screenshot
This is usually caused by 3rd-party plugins that change the value of "GL.invertCulling" (e.g. mirrors). See the solution: https://forum.unity.com/threads/360-screenshot-capture-open-source.501310/#post-7078093

- 360° screenshot is blank on Oculus Quest 2
Try using the CaptureAsync function instead of Capture.