using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HEnum
{
    /// <summary>
    /// 通讯类型枚举
    /// <para>用于区分设备/链路的底层通讯方式（客户端 / 服务端已分开）。</para>
    /// <para>注意：显式赋了数值，便于持久化与后续扩展；新增项请追加在末尾，不要打乱已有值。</para>
    /// </summary>
    [Serializable]
    public enum HSocketType
    {
        /// <summary>
        /// 未指定 / 未知
        /// </summary>
        None = 0,

        // ==================== 网络：Socket 类 (1 ~ 19) ====================

        /// <summary>
        /// TCP 客户端（主动连接远端）
        /// <para>使用字段：IP、Port（HostName 可选）</para>
        /// </summary>
        TCP_Client = 1,

        /// <summary>
        /// TCP 服务端（本地监听，等待客户端连接）
        /// <para>使用字段：IP（通常为 0.0.0.0）、Port</para>
        /// </summary>
        TCP_Server = 2,

        /// <summary>
        /// UDP 客户端（主动发送 / 请求，被动接收回复）
        /// <para>使用字段：IP、Port</para>
        /// </summary>
        UDP_Client = 3,

        /// <summary>
        /// UDP 服务端（绑定本地端口，被动接收）
        /// <para>使用字段：IP（通常为 0.0.0.0）、Port</para>
        /// </summary>
        UDP_Server = 4,

        /// <summary>
        /// UDP 组播（同时收发组播报文，无严格客户端/服务端之分）
        /// <para>使用字段：IP（组播地址，如 239.0.0.1）、Port</para>
        /// </summary>
        UDP_Multicast = 5,

        /// <summary>
        /// WebSocket 客户端（ws:// 或 wss://）
        /// <para>使用字段：IP / HostName、Port、路径</para>
        /// </summary>
        WebSocket_Client = 6,

        /// <summary>
        /// WebSocket 服务端（本地监听）
        /// <para>使用字段：IP（通常为 0.0.0.0）、Port、路径</para>
        /// </summary>
        WebSocket_Server = 7,

        // ==================== 串口 / 总线 (20 ~ 39) ====================
        // 说明：物理链路本身点对点，无网络 C/S 概念；
        //       此处的 Client/Server 表示应用层"主动方 / 被动方"角色。

        /// <summary>
        /// 串口 客户端 / 主动方（主动发送、轮询、发起握手）
        /// <para>使用字段：PortName、BaudRate、DataBits、StopBits、Parity、Handshake</para>
        /// </summary>
        SerialPort_Client = 20,

        /// <summary>
        /// 串口 服务端 / 被动方（等待接收、响应请求）
        /// <para>使用字段：PortName、BaudRate、DataBits、StopBits、Parity、Handshake</para>
        /// </summary>
        SerialPort_Server = 21,

        /// <summary>
        /// USB 转串口（HID / CDC）客户端 / 主动方
        /// </summary>
        Usb_Client = 22,

        /// <summary>
        /// USB 转串口（HID / CDC）服务端 / 被动方
        /// </summary>
        Usb_Server = 23,

        /// <summary>
        /// CAN 总线 客户端 / 主动方（需硬件支持，如 CANalyst、ZLG 等）
        /// </summary>
        CAN_Client = 24,

        /// <summary>
        /// CAN 总线 服务端 / 被动方
        /// </summary>
        CAN_Server = 25,

        // ==================== 工业协议 (40 ~ 59) ====================

        /// <summary>
        /// Modbus TCP 客户端（主站，默认端口 502）
        /// </summary>
        ModbusTCP_Client = 40,

        /// <summary>
        /// Modbus TCP 服务端（从站 / 模拟器，默认端口 502）
        /// </summary>
        ModbusTCP_Server = 41,

        /// <summary>
        /// Modbus RTU 主站（走串口，主动轮询）
        /// </summary>
        ModbusRTU_Master = 42,

        /// <summary>
        /// Modbus RTU 从站（走串口，被动响应）
        /// </summary>
        ModbusRTU_Slave = 43,

        /// <summary>
        /// Modbus RTU over TCP 客户端（RTU 帧走 TCP 传输）
        /// </summary>
        ModbusRTU_TCP_Client = 44,

        /// <summary>
        /// Modbus RTU over TCP 服务端
        /// </summary>
        ModbusRTU_TCP_Server = 45,

        /// <summary>
        /// MQTT 客户端（发布 / 订阅，默认端口 1883 / TLS 8883）
        /// </summary>
        MQTT_Client = 46,

        /// <summary>
        /// MQTT Broker / 服务端（默认端口 1883 / TLS 8883）
        /// </summary>
        MQTT_Broker = 47,

        /// <summary>
        /// OPC UA 客户端（默认端口 4840）
        /// </summary>
        OPCUA_Client = 48,

        /// <summary>
        /// OPC UA 服务端（默认端口 4840）
        /// </summary>
        OPCUA_Server = 49,

        /// <summary>
        /// Siemens S7 客户端（默认端口 102）
        /// </summary>
        SiemensS7_Client = 50,

        /// <summary>
        /// Siemens S7 服务端（模拟 PLC，默认端口 102）
        /// </summary>
        SiemensS7_Server = 51,

        /// <summary>
        /// 三菱 MC 客户端
        /// </summary>
        MitsubishiMC_Client = 52,

        /// <summary>
        /// 三菱 MC 服务端
        /// </summary>
        MitsubishiMC_Server = 53,

        /// <summary>
        /// 欧姆龙 FINS 客户端
        /// </summary>
        OmronFINS_Client = 54,

        /// <summary>
        /// 欧姆龙 FINS 服务端
        /// </summary>
        OmronFINS_Server = 55,

        // ==================== 其它 (60 ~ 98) ====================

        /// <summary>
        /// HTTP / HTTPS 客户端（请求方）
        /// </summary>
        HTTP_Client = 60,

        /// <summary>
        /// HTTP / HTTPS 服务端（监听方）
        /// </summary>
        HTTP_Server = 61,

        /// <summary>
        /// 命名管道 客户端 / 本地进程间通讯
        /// </summary>
        NamedPipe_Client = 62,

        /// <summary>
        /// 命名管道 服务端 / 本地进程间通讯
        /// </summary>
        NamedPipe_Server = 63,

        /// <summary>
        /// 自定义协议（由上层自行解析）
        /// </summary>
        Custom = 99
    }
}