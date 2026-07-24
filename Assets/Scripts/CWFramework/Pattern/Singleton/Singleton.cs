namespace CWFramework
{
	public class Singleton<T> where T : class, new()
	{
		private static T instance;

		private static readonly object padlock = new object();

		public static T Instance
		{
			get
			{
				if (null == instance)
				{
					lock (padlock)
					{
						if (null == instance)
						{
							instance = new T();
						}
					}
				}

				return instance;
			}
		}
	}
}