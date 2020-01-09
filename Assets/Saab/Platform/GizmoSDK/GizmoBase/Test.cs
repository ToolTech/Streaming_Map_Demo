//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s)
// herein belong to SAAB AB. The program(s) may be used and/or
// copied only with the written permission of SAAB AB, or in
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
// File			: Test.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzTestAssetManager class
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.5
//		
//
//			
// NOTE:	GizmoBase is a platform abstraction utility layer for C++. It contains 
//			design patterns and C++ solutions for the advanced programmer.
//
//
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	180301	Created file 	
//
//******************************************************************************

using System;
using System.Collections.Generic;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public struct TestAssertInfo
        {
            public TestAssertInfo(string _source , string _message)
            {
                Source = _source;
                Message = _message;
            }

            public string Source;
            public string Message;
        }

        public class TestAssertManager
        {
            public TestAssertManager()
            {
                Message.OnMessage += Message_OnMessage;
            }

            private void Message_OnMessage(string sender, MessageLevel level, string message)
            {
                if ((level & MessageLevel.LEVEL_MASK_STD) == MessageLevel.ASSERT)
                    _asserts.Add(new TestAssertInfo (sender, message ));
                else if (AssertOnFatal && ((level & MessageLevel.LEVEL_MASK_STD) == MessageLevel.FATAL))
                    _asserts.Add(new TestAssertInfo(sender, message));
                else if (AssertOnWarning && ((level & MessageLevel.LEVEL_MASK_STD) == MessageLevel.WARNING))
                     _asserts.Add(new TestAssertInfo(sender, message));
            }

            public void ReportErrors()
            {
                foreach(TestAssertInfo info in _asserts)
                {
                    DynamicType dyn = DynamicType.FromXML(info.Message);

                    //if(dyn.IsValid())
                    //{
                    //    if(dyn.IsVoid())
                    //        throw new Exception($"Source:{info.Source} Message:{info.Message}");
                    //    else if(dyn.Is(DynamicType.Type.CONTAINER))
                    //    {
                    //        DynamicTypeContainer cont = dyn;

                    //    }
                    //}
                    //else

                    throw new Exception($"Source:{info.Source} Message:{info.Message}");
                }

                _asserts.Clear();
            }

            public void ExpectedException<T>() where T : Exception
            {
                if(_asserts.Count==0)
                    throw new Exception($"Missing Exception:{typeof(T).Name}");

                TestAssertInfo info = _asserts[0];

                DynamicType dyn = DynamicType.FromXML(info.Message);

                if (dyn.IsValid())
                {
                    if (dyn.IsVoid())
                        throw new Exception($"Source:{info.Source} Message:{info.Message}");
                    else if (dyn.Is(DynamicType.Type.CONTAINER))
                    {
                        DynamicTypeContainer cont = dyn;

                        if(cont.GetAttribute("Type")!=typeof(T).Name)
                            throw new Exception($"Source:{info.Source} Message:{info.Message}");
                    }
                }
                else
                    throw new Exception($"Source:{info.Source} Message:{info.Message}");
            }

            public bool AssertOnWarning = true;

            public bool AssertOnFatal = true;

            List<TestAssertInfo> _asserts= new List<TestAssertInfo>();
        }


    }
}

