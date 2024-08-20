using System.Runtime.InteropServices;

namespace SharpPipe;


[StructLayout(LayoutKind.Sequential)]
public unsafe struct sp_zitarev
{
    void* faust;
    public int argpos;
    internal sp_zitarev_args args;
    public float* in_delay;
    public float* lf_x;
    public float* rt60_low;
    public float* rt60_mid;
    public float* hf_damping;
    public float* eq1_freq;
    public float* eq1_level;
    public float* eq2_freq;
    public float* eq2_level;
    public float* mix;
    public float* level;
}


[StructLayout(LayoutKind.Sequential)]
public unsafe struct sp_zitarev_args
{
    public float* arg0;
    public float* arg1;
    public float* arg2;
    public float* arg3;
    public float* arg4;
    public float* arg5;
    public float* arg6;
    public float* arg7;
    public float* arg8;
    public float* arg9;
    public float* arg10;
}