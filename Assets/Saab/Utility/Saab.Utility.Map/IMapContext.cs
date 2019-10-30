using System;

namespace Saab.Utility.Map
{
	public struct Float3
	{ 
		public float X;
		public float Y;
		public float Z;
	}
	
    public interface ILocation<TContext>
    {
        TContext Context { get; }
        Float3 Offset { get; }
    }

    public enum PositionOptions
    {
        Ellipsoid,
        Surface,
    }

    public enum LoadOptions
    {
        DontLoad,
        Load,
    }

    public enum QualityOptions
    {
        Default,
        Highest,
    }

    public struct LocationOptions
    {
        public PositionOptions PositionOptions;
        public LoadOptions LoadOptions;
        public QualityOptions QualityOptions;

        public static LocationOptions Default = new LocationOptions()
        {
            PositionOptions = PositionOptions.Surface,
            LoadOptions = LoadOptions.DontLoad,
            QualityOptions = QualityOptions.Default,
        };
    }

    

    public interface IMapLocationProvider<TLocation, TContext> where TLocation : ILocation<TContext>
    {
        bool GetContext(double lat, double lon, double alt, LocationOptions options, out TLocation location);
    }
}
