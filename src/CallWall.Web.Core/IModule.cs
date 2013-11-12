using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallWall.Web
{
    public interface IModule
    {
        //Need to be able to Register singleton, instance, and composite
        void Initialise(ITypeRegistry registry);
    }

    public interface ITypeRegistry
    {
        void RegisterType<TFrom, TTo>() where TTo : TFrom;
        void RegisterType<TFrom, TTo>(string name) where TTo : TFrom;
    }
}
