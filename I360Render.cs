using System;
using UnityEngine;

public class I360Render : MonoBehaviour 
{
	private static Material equirectangularConverter = null;

	public static byte[] Capture( int width = 1024, Camera renderCam = null )
	{
		if( renderCam == null )
		{
			renderCam = Camera.main;
			if( renderCam == null )
			{
				Debug.LogError( "Error: no camera detected" );
				return null;
			}
		}

		RenderTexture camTarget = renderCam.targetTexture;

		if( equirectangularConverter == null )
			equirectangularConverter = new Material( Shader.Find( "Hidden/CubemapToEquirectangular" ) );

		int cubemapSize = Mathf.Min( Mathf.NextPowerOfTwo( width ), 8192 );
		RenderTexture cubemap = null, equirectangularTexture = null;
		Texture2D output = null;
		try
		{
			cubemap = RenderTexture.GetTemporary( cubemapSize, cubemapSize, 0 );
			cubemap.dimension = UnityEngine.Rendering.TextureDimension.Cube;

			equirectangularTexture = RenderTexture.GetTemporary( cubemapSize, cubemapSize / 2, 0 );
			equirectangularTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;

			if( !renderCam.RenderToCubemap( cubemap, 63 ) )
			{
				Debug.LogError( "Rendering to cubemap is not supported on device/platform!" );
				return null;
			}

			Graphics.Blit( cubemap, equirectangularTexture, equirectangularConverter );
			
			RenderTexture temp = RenderTexture.active;
			RenderTexture.active = equirectangularTexture;
			output = new Texture2D( equirectangularTexture.width, equirectangularTexture.height, TextureFormat.RGB24, false );
			output.ReadPixels( new Rect( 0, 0, equirectangularTexture.width, equirectangularTexture.height ), 0, 0 );
			RenderTexture.active = temp;
			
			return InsertXMPIntoTexture2D_JPEG( output );
		}
		catch( Exception e )
		{
			Debug.LogException( e );

			return null;
		}
		finally
		{
			renderCam.targetTexture = camTarget;

			if( cubemap != null )
				RenderTexture.ReleaseTemporary( cubemap );

			if( equirectangularTexture != null )
				RenderTexture.ReleaseTemporary( equirectangularTexture );

			if( output != null )
				Destroy( output );
		}
	}

	#region JPEG XMP Injection
	private const string XMP_NAMESPACE_JPEG = "http://ns.adobe.com/xap/1.0/";
	private const string XMP_CONTENT_TO_FORMAT_JPEG = "<x:xmpmeta xmlns:x=\"adobe:ns:meta/\" x:xmptk=\"Adobe XMP Core 5.1.0-jc003\"> <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"> <rdf:Description rdf:about=\"\" xmlns:GPano=\"http://ns.google.com/photos/1.0/panorama/\" GPano:UsePanoramaViewer=\"True\" GPano:CaptureSoftware=\"Unity3D\" GPano:StitchingSoftware=\"Unity3D\" GPano:ProjectionType=\"equirectangular\" GPano:PoseHeadingDegrees=\"180.0\" GPano:InitialViewHeadingDegrees=\"0.0\" GPano:InitialViewPitchDegrees=\"0.0\" GPano:InitialViewRollDegrees=\"0.0\" GPano:InitialHorizontalFOVDegrees=\"{0}\" GPano:CroppedAreaLeftPixels=\"0\" GPano:CroppedAreaTopPixels=\"0\" GPano:CroppedAreaImageWidthPixels=\"{1}\" GPano:CroppedAreaImageHeightPixels=\"{2}\" GPano:FullPanoWidthPixels=\"{1}\" GPano:FullPanoHeightPixels=\"{2}\"/></rdf:RDF></x:xmpmeta>";


	public static byte[] InsertXMPIntoTexture2D_JPEG( Texture2D image )
	{
		byte[] fileBytes = image.EncodeToJPG( 100 );

		return DoTheHardWork_JPEG( fileBytes, image.width, image.height );
	}

	private static byte[] DoTheHardWork_JPEG( byte[] fileBytes, int imageWidth, int imageHeight )
	{
		int xmpIndex = 0, xmpContentSize = 0;
		while( !SearchChunkForXMP_JPEG( fileBytes, ref xmpIndex, ref xmpContentSize ) )
		{
			if( xmpIndex == -1 )
				break;
		}

		int copyBytesUntil, copyBytesFrom;
		if( xmpIndex == -1 )
		{
			copyBytesUntil = copyBytesFrom = FindIndexToInsertXMPCode_JPEG( fileBytes );
		}
		else
		{
			copyBytesUntil = xmpIndex;
			copyBytesFrom = xmpIndex + 2 + xmpContentSize;
		}

		string xmpContent = string.Concat( XMP_NAMESPACE_JPEG, "\0", string.Format( XMP_CONTENT_TO_FORMAT_JPEG, 75f.ToString( "F1" ), imageWidth, imageHeight ) );
		int xmpLength = xmpContent.Length + 2;
		xmpContent = string.Concat( (char) 0xFF, (char) 0xE1, (char) ( xmpLength / 256 ), (char) ( xmpLength % 256 ), xmpContent );

		byte[] result = new byte[copyBytesUntil + xmpContent.Length + ( fileBytes.Length - copyBytesFrom )];

		Array.Copy( fileBytes, 0, result, 0, copyBytesUntil );

		for( int i = 0; i < xmpContent.Length; i++ )
		{
			result[copyBytesUntil + i] = (byte) xmpContent[i];
		}

		Array.Copy( fileBytes, copyBytesFrom, result, copyBytesUntil + xmpContent.Length, fileBytes.Length - copyBytesFrom );

		return result;
	}

	private static bool CheckBytesForXMPNamespace_JPEG( byte[] bytes, int startIndex )
	{
		for( int i = 0; i < XMP_NAMESPACE_JPEG.Length; i++ )
		{
			if( bytes[startIndex + i] != XMP_NAMESPACE_JPEG[i] )
				return false;
		}

		return true;
	}

	private static bool SearchChunkForXMP_JPEG( byte[] bytes, ref int startIndex, ref int chunkSize )
	{
		if( startIndex + 4 < bytes.Length )
		{
			//Debug.Log( startIndex + " " + System.Convert.ToByte( bytes[startIndex] ).ToString( "x2" ) + " " + System.Convert.ToByte( bytes[startIndex+1] ).ToString( "x2" ) + " " +
			//           System.Convert.ToByte( bytes[startIndex+2] ).ToString( "x2" ) + " " + System.Convert.ToByte( bytes[startIndex+3] ).ToString( "x2" ) );

			if( bytes[startIndex] == 0xFF )
			{
				byte secondByte = bytes[startIndex + 1];
				if( secondByte == 0xDA )
				{
					startIndex = -1;
					return false;
				}
				else if( secondByte == 0x01 || ( secondByte >= 0xD0 && secondByte <= 0xD9 ) )
				{
					startIndex += 2;
					return false;
				}
				else
				{
					chunkSize = bytes[startIndex + 2] * 256 + bytes[startIndex + 3];

					if( secondByte == 0xE1 && chunkSize >= 31 && CheckBytesForXMPNamespace_JPEG( bytes, startIndex + 4 ) )
					{
						return true;
					}

					startIndex = startIndex + 2 + chunkSize;
				}
			}
		}

		return false;
	}

	private static int FindIndexToInsertXMPCode_JPEG( byte[] bytes )
	{
		int chunkSize = bytes[4] * 256 + bytes[5];
		return chunkSize + 4;
	}
	#endregion
}