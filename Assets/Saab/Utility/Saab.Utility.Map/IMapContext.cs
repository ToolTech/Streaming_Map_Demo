//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s) 
// herein belong to Saab AB. The program(s) may be used and/or
// copied only with the written permission of Saab AB, or in
// accordance with the terms and conditions stipulated in the
// agreement/contract under which the program(s) have been
// supplied. 
//
//
// Information Class:	COMPANY UNCLASSIFIED
// Defence Secrecy:		NOT CLASSIFIED
// Export Control:		NOT EXPORT CONTROLLED
//
//
// File			: IMapContext.cs
// Module		:
// Description	: Interfaces for Map Context control
// Author		: Anders Modén
// Product		: Gizmo3D 2.10.1
//
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows (Win32) and IRIX® for  
//			usage in Game or VisSim development.
//
//
// Revision History...
//
// Who	Date	Description
//
// AMO	190601	Created file                                        (2.10.1)
//
//******************************************************************************
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
