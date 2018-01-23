#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX || UNITY_STANDALONE || UNITY_STANDALONE_LINUX_API
#define IMPORT_GLENABLE
#endif

//"/usr/lib/nvidia-361/libGL.so";
using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class EnablePointSize : MonoBehaviour
{
    const UInt32 GL_VERTEX_PROGRAM_POINT_SIZE = 0x8642;
    const UInt32 GL_POINT_SMOOTH = 0x0B10;

    const string LibGLPath =

#if UNITY_EDITOR_LINUX
         "/usr/lib/x86_64-linux-gnu/mesa/libGL.so" ;// Untested on Linux, this may not be correct
#elif UNITY_STANDALONE_LINUX
         "/usr/lib/x86_64-linux-gnu/mesa/libGL.so" ;// Untested on Linux, this may not be correct
#elif UNITY_STANDALONE_WIN
         "opengl32.dll";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
         "/System/Library/Frameworks/OpenGL.framework/OpenGL";
 
#else
         null;   // OpenGL ES platforms don't require this feature
#endif

#if IMPORT_GLENABLE
    [DllImport(LibGLPath)]
    public static extern void glEnable(UInt32 cap);

    private bool mIsOpenGL;
    private bool first;

    void Start()
    {
        mIsOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
        first = true;

#if UNITY_EDITOR_LINUX
         Debug.LogError("unity editor");
#elif UNITY_STANDALONE_LINUX
         Debug.LogError ("unity standalone linux");
#endif

    }

    void OnPreRender()
    {
        if (first)
        {   
            if (mIsOpenGL)
                glEnable(GL_VERTEX_PROGRAM_POINT_SIZE);
            first = false;
            glEnable(GL_POINT_SMOOTH);
        }
    }
#endif
}