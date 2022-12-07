#if UNITY_IOS
using System.Runtime.InteropServices;
using UnityEngine;

namespace WorldAsSupport {
	/// <summary>
	///     Class to interact with flashlight
	/// </summary>
	public static class NativeFlashlight
	{
		/// <summary>
		/// 	Indicates whether current device has a flashlight
		/// </summary>
		/// <returns><code>true</code> if current device has a flashlight, <code>false</code> otherwise</returns>
		public static bool HasTorch
		{
			get
			{
				return _DeviceHasFlashlight();
			}
		}

		/// <summary>
		///     Toggle the flashlight
		/// </summary>
		/// <param name="enable">Whether to enable or disable the flashlight</param>
		public static void EnableFlashlight(bool enable)
		{
			_EnableFlashlight(enable);
		}

		/// <summary>
		/// Enables flashlight with the provided intensity
		/// </summary>
		/// <param name="intensity">Intensity of the flashlight to set. Clamped between 0 and 1</param>
		public static void SetFlashlightIntensity(float intensity)
		{
			intensity = Mathf.Clamp01(intensity);
			_SetFlashlightLevel(intensity);
		}

		[DllImport("__Internal")]
		static extern void _EnableFlashlight(bool enable);

		[DllImport("__Internal")]
		static extern void _SetFlashlightLevel(float val);

		[DllImport("__Internal")]
		static extern bool _DeviceHasFlashlight();
	}
}
#endif
