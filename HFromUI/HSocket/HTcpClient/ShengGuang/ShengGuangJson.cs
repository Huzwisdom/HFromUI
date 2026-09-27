using System.Collections.Generic;
using HFromUI.HAttribute;

namespace HFromUI.HSocket.HTcpClient.ShengGuang
{
    /// <summary>设备状态根对象</summary>
    public class DeviceStatus
    {
        [HName("deviceId")]
        public string DeviceId { get; set; }

        [HName("machineNo")]
        public string MachineNo { get; set; }

        [HName("machineStatus")]
        public string MachineStatus { get; set; }

        [HName("stage")]
        public string Stage { get; set; }

        [HName("online")]
        public bool Online { get; set; }

        [HName("hasAlarm")]
        public bool HasAlarm { get; set; }

        [HName("lamp")]
        public Lamp Lamp { get; set; }

        [HName("job")]
        public Job Job { get; set; }

        [HName("temperature")]
        public Temperature Temperature { get; set; }=new Temperature();

        [HName("code")]
        public string Code { get; set; }

        [HName("category")]
        public string Category { get; set; }

        [HName("description")]
        public string Description { get; set; }

        [HName("result")]
        public Result Result { get; set; }
    }

    /// <summary>信号灯状态</summary>
    public class Lamp
    {
        [HName("red")] public bool Red { get; set; }
        [HName("yellow")] public bool Yellow { get; set; }
        [HName("green")] public bool Green { get; set; }
        [HName("buzzer")] public bool Buzzer { get; set; }
    }

    /// <summary>当前作业信息</summary>
    public class Job
    {
        [HName("orderNo")] public string OrderNo { get; set; }
        [HName("waferNo")] public string WaferNo { get; set; }
        [HName("operation")] public string Operation { get; set; }
        [HName("planName")] public string PlanName { get; set; }
        // "operator" 是 C# 关键字，属性名用 Operator，靠 HName 对应回去
        [HName("operator")] public string Operator { get; set; }
    }

    /// <summary>温度信息</summary>
    public class Temperature
    {
        [HName("setValue")] public double SetValue { get; set; }

        [HName("actual")] public Actual Actual { get; set; }
    }

    /// <summary>实际温度（各通道）</summary>
    public class Actual
    {
        [HName("U1")] public double U1 { get; set; }
        [HName("U2")] public double U2 { get; set; }
    }

    /// <summary>测量结果集合</summary>
    public class Result
    {
        [HName("measurements")]
        public List<Measurement> Measurements { get; set; }
    }

    /// <summary>单条测量项</summary>
    public class Measurement
    {
        [HName("name")] public string Name { get; set; }
        [HName("value")] public double Value { get; set; }
        [HName("unit")] public string Unit { get; set; }
        [HName("precision")] public double Precision { get; set; }
    }
}