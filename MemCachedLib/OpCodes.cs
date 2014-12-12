using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedLib
{
    /// <summary>
    /// 操作指令     
    /// </summary>
    internal enum OpCodes : byte
    {
        Get = 0x00,
        Set = 0x01,
        Add = 0x02,
        Replace = 0x03,
        Delete = 0x04,
        Increment = 0x05,
        Decrement = 0x06,
        Quit = 0x07,
        Flush = 0x08,
        GetQ = 0x09,
        No_op = 0x0a,
        Version = 0x0b,
        GetK = 0x0c,
        GetKQ = 0x0d,
        Append = 0x0e,
        Prepend = 0x0f,
        Stat = 0x10,
        SetQ = 0x11,
        AddQ = 0x12,
        ReplaceQ = 0x13,
        DeleteQ = 0x14,
        IncrementQ = 0x15,
        DecrementQ = 0x16,
        QuitQ = 0x17,
        FlushQ = 0x18,
        AppendQ = 0x19,
        PrependQ = 0x1a,
        Verbosity = 0x1b,
        Touch = 0x1c,
        GAT = 0x1d,
        GATQ = 0x1e,
        SASL_List_Mechs = 0x20,
        SASL_Auth = 0x21,
        SASL_Step = 0x22,
        RGet = 0x30,
        RSet = 0x31,
        RSetQ = 0x32,
        RAppend = 0x33,
        RAppendQ = 0x34,
        RPrepend = 0x35,
        RPrependQ = 0x36,
        RDelete = 0x37,
        RDeleteQ = 0x38,
        RIncr = 0x39,
        RIncrQ = 0x3a,
        RDecr = 0x3b,
        RDecrQ = 0x3c,
        Set_VBucket = 0x3d,
        Get_VBucket = 0x3e,
        Del_VBucket = 0x3f,
        TA_Connect = 0x40,
        TAP_Mutation = 0x41,
        TAP_Delete = 0x42,
        TAP_Flush = 0x43,
        TAP_Opaque = 0x44,
        TAP_VBucket_Set = 0x45,
        TAP_Checkpoint_Start = 0x46,
        TAP_Checkpoint_End = 0x47,
    }
}
