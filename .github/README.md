# Unity 360° Screenshot Capture

**Available on Asset Store:** https://assetstore.unity.com/packages/tools/camera/360-screenshot-capture-112864

**Forum Thread:** https://forum.unity.com/threads/360-screenshot-capture-open-source.501310/

**[Support the Developer ☕](https://yasirkula.itch.io/unity3d)**

This simple script captures a **360° photo** with your Unity camera and injects the necessary **XMP metadata** to it; so the output image supports 360° viewers on the web out-of-the-box (like *Facebook* and *Flickr*). Both **JPEG** and **PNG** formats are supported.

The raw image is in equirectangular form. Here is an example screenshot [(it looks like this when uploaded to Flickr)](https://flic.kr/p/VPxPwY):

![screenshot](Images/360render.jpeg)

## INSTALLATION

There are 4 ways to install this plugin:

- import [360Screenshot.unitypackage](https://github.com/yasirkula/Unity360ScreenshotCapture/releases) via *Assets-Import Package*
- clone/[download](https://github.com/yasirkula/Unity360ScreenshotCapture/archive/master.zip) this repository and move the *Plugins* folder to your Unity project's *Assets* folder
- import it from [Asset Store](https://assetstore.unity.com/packages/tools/camera/360-screenshot-capture-112864)
- *(via Package Manager)* add the following line to *Packages/manifest.json*:
  - `"com.yasirkula.screenshotcapture": "https://github.com/yasirkula/Unity360ScreenshotCapture.git",`

## HOW TO

Simply call the `I360Render.Capture()` function in your scripts. Its signature is as following:

```csharp
public static byte[] Capture( int width = 1024, bool encodeAsJPEG = true, Camera renderCam = null, bool faceCameraDirection = true );
```

- **width**: The width of the resulting image. It must be a power of 2. The height will be equal to *width / 2*. Be aware that maximum allowed image width is 8192 pixels
- **encodeAsJPEG**: determines whether the image will be encoded as *JPEG* or *PNG*
- **renderCam**: the camera that will be used to render the 360° image. If set to null, *Camera.main* will be used
- **faceCameraDirection**: if set to *true*, when the 360° image is viewed in a 360° viewer, initial camera rotation will match the rotation of the *renderCam*. Otherwise, initial camera rotation will be *Quaternion.identity* (facing Z+ axis)

The function returns a **byte[]** object that you can write to a file using `File.WriteAllBytes` (see example code below).

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
