using UnityEngine;

namespace Checkers
{
	public class DataConfig : Singleton<DataConfig>
	{
		[Tooltip("How many time game wait after click on NoAdsButton and showing rewarded video.")]
		public float NoAdsRewardAppearDelay = 3f;
		[Tooltip("Delay before AI move.")]
		public float AIMoveTime = 1f;
	}
}