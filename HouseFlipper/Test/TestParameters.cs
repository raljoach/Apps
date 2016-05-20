using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseFlipper.Test
{
    public enum DataFolder
    {
        Default,
        NotExistent,
        Exists
    }

    public enum Files
    {
        UnusedParameter,
        NotExistent,
        Exists
    }

    public enum FileCount
    {
        UnusedParameter,
        Single
    }

    /*
    public enum RowCount
    {
        UnusedParameter,
        Empty,
        Single
    }
    */

    public enum HeaderCount
    {
        UnusedParameter,
        None,
        Single,
        Multiple
    }


    public enum DataRowCount
    {
        UnusedParameter,
        None,
        Single,
        Multiple
    }


    public enum FieldCount
    {
        UnusedParameter,
        Normal, //SameAsHeader,
        LessThanHeader,
        MoreThanHeader,        
    }
    /*
    public enum FieldValueType
    {
        String,
        Number,        
    }

    public enum Comma
    {
        None,
        Only,
        Start,
        Middle,
        End
    }

    public class Field
    {
        public bool Quoted;
        public FieldValueType ValueType;
        public Comma Comma;  
    }*/
}
