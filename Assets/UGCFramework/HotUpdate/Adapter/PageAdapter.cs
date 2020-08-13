using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using UGCF.Manager;

namespace UGCF.HotUpdate
{
    public class PageAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mRefreshAdaptUI_0 = new CrossBindingMethodInfo("RefreshAdaptUI");
        static CrossBindingMethodInfo mInit_1 = new CrossBindingMethodInfo("Init");
        static CrossBindingMethodInfo mOpen_2 = new CrossBindingMethodInfo("Open");
        static CrossBindingMethodInfo mClose_3 = new CrossBindingMethodInfo("Close");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(Page);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : Page, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public override void Init()
            {
                if (mInit_1.CheckShouldInvokeBase(instance))
                    base.Init();
                else
                    mInit_1.Invoke(instance);
            }

            public override void Open()
            {
                if (mOpen_2.CheckShouldInvokeBase(instance))
                    base.Open();
                else
                    mOpen_2.Invoke(instance);
            }

            public override void Close()
            {
                if (mClose_3.CheckShouldInvokeBase(instance))
                    base.Close();
                else
                    mClose_3.Invoke(instance);
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
    }
}

