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

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************

using System;
using System.Numerics;

namespace Saab.Utility.Map
{
    
    public struct Transform
    {
        public Vector3 Pos;
        public Quaternion Rot;
    }

    public interface ILocationMethods
    {
        Transform Step(double time, LocationOptions options);
    }



    public interface ILocation<TContext> : ILocationMethods
    {
        TContext Context { get; }
        Vector3 LocalPosition { get; }
        Quaternion Rotation { get; }
    }

    public interface IMapLocation<TContext> : ILocation<TContext>
    {
        bool SetLatPos(double lat, double lon, double alt);
        void SetRotation(float yaw, float pitch, float roll);
    }

    public interface IDynamicLocation<TContext> : IMapLocation<TContext>
    {
        bool SetKinematicParams(double posX, double posY, double posZ, Vector3 vel, Vector3 acc, double t);
    }


    public enum PositionOptions : byte
    {
        None,
        Ellipsoid,
        Surface,
    }

    public enum LoadOptions : byte
    {
        DontLoad,
        Load,
    }

    public enum QualityOptions : byte
    {
        Default,
        Highest,
    }
    [Flags]
    public enum RotationOptions : sbyte
    {
        None = 0,
        AlignToSurface = 1 << 1,
        AlignToVelocity = 1 << 2,

        AlignToVelocityAndSurface = AlignToVelocity | AlignToSurface,
        Default = AlignToVelocity
    }

    [Serializable]
    public struct LocationOptions
    {
        public PositionOptions PositionOptions;
        public LoadOptions LoadOptions;
        public QualityOptions QualityOptions;
        public RotationOptions RotationOptions;

        public static readonly LocationOptions Default = new LocationOptions()
        {
            PositionOptions = PositionOptions.Surface,
            LoadOptions = LoadOptions.DontLoad,
            QualityOptions = QualityOptions.Default,
            RotationOptions = RotationOptions.Default
        };
    }

    

    public interface IMapLocationProvider<TLocation,TContext> where TLocation : ILocation<TContext>
    {
        bool CreateLocation(out TLocation location);
    }
}
