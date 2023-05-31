# Unity 360° Screenshot Capture

**Available on Asset Store:** https://assetstore.unity.com/packages/tools/camera/360-screenshot-capture-112864

**Forum Thread:** https://forum.unity.com/threads/360-screenshot-capture-open-source.501310/

**Discord:** https://discord.gg/UJJt549AaV

**[GitHub Sponsors ☕](https://github.com/sponsors/yasirkula)**

This simple script captures a **360° photo** with your Unity camera and injects the necessary **XMP metadata** to it; so the output image supports 360° viewers on the web out-of-the-box (like *Facebook* and *Flickr*). Both **JPEG** and **PNG** formats are supported.

The raw image is in equirectangular form. Here is an example screenshot [(it looks like this when uploaded to Flickr)](https://flic.kr/p/VPxPwY):

![screenshot](Images/360render.jpeg)

## INSTALLATION

There are 5 ways to install this plugin:

- import [360Screenshot.unitypackage](https://github.com/yasirkula/Unity360ScreenshotCapture/releases) via *Assets-Import Package*
- clone/[download](https://github.com/yasirkula/Unity360ScreenshotCapture/archive/master.zip) this repository and move the *Plugins* folder to your Unity project's *Assets* folder
- import it from [Asset Store](https://assetstore.unity.com/packages/tools/camera/360-screenshot-capture-112864)
- *(via Package Manager)* add the following line to *Packages/manifest.json*:
  - `"com.yasirkula.screenshotcapture": "https://github.com/yasirkula/Unity360ScreenshotCapture.git",`
- *(via [OpenUPM](https://openupm.com))* after installing [openupm-cli](https://github.com/openupm/openupm-cli), run the following command:
  - `openupm add com.yasirkula.screenshotcapture`

## HOW TO

Simply call the `I360Render.Capture()` or `I360Render.CaptureAsync()` (Unity 2018.2 or later) function in your scripts. Their signatures are as follows:

```csharp
public static byte[] Capture( int width = 1024, bool encodeAsJPEG = true, Camera renderCam = null, bool faceCameraDirection = true );

// !!! Async version uses AsyncGPUReadback.Request so it won't work on all platforms or Graphics APIs !!!
public static void CaptureAsync( Action<byte[]> callback, int width = 1024, bool encodeAsJPEG = true, Camera renderCam = null, bool faceCameraDirection = true );
```

- **width**: The width of the resulting image. It must be a power of 2. The height will be equal to *width / 2*. Be aware that maximum allowed image width is 8192 pixels
- **encodeAsJPEG**: determines whether the image will be encoded as *JPEG* or *PNG*
- **renderCam**: the camera that will be used to render the 360° image. If set to null, *Camera.main* will be used
- **faceCameraDirection**: if set to *true*, when the 360° image is viewed in a 360° viewer, initial camera rotation will match the rotation of the *renderCam*. Otherwise, initial camera rotation will be *Quaternion.identity* (facing Z+ axis)

These functions return a **byte[]** object either directly or as a callback; you can write these bytes to a file using `File.WriteAllBytes` (see example code below).

## FAQ

- **Objects are rendered inside out in the 360° screenshot**

This is usually caused by 3rd-party plugins that change the value of `GL.invertCulling` (e.g. mirrors). See the solution: https://forum.unity.com/threads/360-screenshot-capture-open-source.501310/#post-7078093

- **360° screenshot is blank on Oculus Quest 2**

Try using the `CaptureAsync` function instead of `Capture`.

## EXAMPLE CODE

```csharp
using System.IO;
using UnityEngine;

public class RenderTest : MonoBehaviour
{
	public int imageWidth = 1024;
	public bool saveAsJPEG = true;

	void Update()
	{
		if( Input.GetKeyDown( KeyCode.P ) )
		{
			byte[] bytes = I360Render.Capture( imageWidth, saveAsJPEG );
			if( bytes != null )
			{
				string path = Path.Combine( Application.persistentDataPath, "360render" + ( saveAsJPEG ? ".jpeg" : ".png" ) );
				File.WriteAllBytes( path, bytes );
				Debug.Log( "360 render saved to " + path );
			}
		}
	}
}
```
