using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using UGCF.Manager;

namespace UGCF.HotUpdate
{
    public class NodeAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mUIAdapt_0 = new CrossBindingMethodInfo("UIAdapt");
        static CrossBindingMethodInfo mInit_1 = new CrossBindingMethodInfo("Init");
        static CrossBindingMethodInfo mOpen_2 = new CrossBindingMethodInfo("Open");
        static CrossBindingMethodInfo mEnterAnimationEndAction_3 = new CrossBindingMethodInfo("EnterAnimationEndAction");
        static CrossBindingMethodInfo mExitAnimationEndAction_4 = new CrossBindingMethodInfo("ExitAnimationEndAction");
        static CrossBindingMethodInfo mRequestData_5 = new CrossBindingMethodInfo("RequestData");
        static CrossBindingMethodInfo<bool> mClose_6 = new CrossBindingMethodInfo<bool>("Close");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(Node);
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

        public class Adapter : Node, CrossBindingAdaptorType
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

            public override void EnterAnimationEndAction()
            {
                if (mEnterAnimationEndAction_3.CheckShouldInvokeBase(instance))
                    base.EnterAnimationEndAction();
                else
                    mEnterAnimationEndAction_3.Invoke(instance);
            }

            public override void ExitAnimationEndAction()
            {
                if (mExitAnimationEndAction_4.CheckShouldInvokeBase(instance))
                    base.ExitAnimationEndAction();
                else
                    mExitAnimationEndAction_4.Invoke(instance);
            }

            public override void RequestData()
            {
                if (mRequestData_5.CheckShouldInvokeBase(instance))
                    base.RequestData();
                else
                    mRequestData_5.Invoke(instance);
            }

            public override void Close(bool isActiveClose)
            {
                if (mClose_6.CheckShouldInvokeBase(instance))
                    base.Close(isActiveClose);
                else
                    mClose_6.Invoke(instance, isActiveClose);
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

