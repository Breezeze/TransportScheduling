//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace TransportScheduling.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class T_Infor_Warehouse
    {
        public int Wid { get; set; }
        public string WName { get; set; }
        public Nullable<int> WStaffId { get; set; }
        public string WLocation { get; set; }
        public Nullable<int> WState { get; set; }
    
        public virtual T_Dic_WarehouseState T_Dic_WarehouseState { get; set; }
    }
}
