﻿namespace TaoTie
{
    public class BlenderEnvironmentRunner: EnvironmentRunner
    {
        public NormalEnvironmentRunner From { get; private set; }
        public NormalEnvironmentRunner To { get; private set; }

        private EnvironmentInfo formData;
        private EnvironmentInfo toData;
        private long startlerpTime;

        private ConfigBlender config;
        private EasingFunction.Function lerpFunc;
        
        public static BlenderEnvironmentRunner Create(NormalEnvironmentRunner from, NormalEnvironmentRunner to, 
            bool isEnter, WeatherSystem weatherSystem)
        {
            BlenderEnvironmentRunner res = ObjectPool.Instance.Fetch<BlenderEnvironmentRunner>();
            res.weatherSystem = weatherSystem;
            res.Id = IdGenerater.Instance.GenerateId();
            res.From = from;
            res.To = to;
            res.Data = EnvironmentInfo.DeepClone(res.From.Data);
            res.Data.IsBlender = true;
            res.Priority = to.Priority;
            if (isEnter)
            {
                res.config = to.Config.Enter;
            }
            else
            {
                res.config = from.Config.Leave;
            }
            if (res.config == null)
                res.config = weatherSystem.DefaultEasing;
            
            res.IsOver = false;
            res.startlerpTime = GameTimerManager.Instance.GetTimeNow();
            return res;
        }

        public override void Update()
        {
            Data.Changed = false;
            if (To.IsOver) IsOver = true;
            if (IsOver) return;

            var time = GameTimerManager.Instance.GetTimeNow();
            if (time > startlerpTime + config.DeltaTime)
            {
                IsOver = true;
            }

            var lerpVal = lerpFunc(time, startlerpTime, startlerpTime + config.DeltaTime);
            Data.Lerp(formData, toData, lerpVal);
            Data.Changed = true;
        }

        /// <summary>
        /// 切换过渡目标
        /// </summary>
        /// <param name="to"></param>
        /// <param name="isEnter"></param>
        public void ChangeTo(NormalEnvironmentRunner to, bool isEnter)
        {
            this.To = to;
            formData = Data;
            Data = EnvironmentInfo.DeepClone(formData);
            
            toData = to.Data;
            
            if (isEnter)
            {
                config = to.Config.Enter;
            }
            else
            {
                config = From.Config.Leave;
            }

            if (config == null)
                config = weatherSystem.DefaultEasing;
            Priority = to.Priority;
            IsOver = false;
            lerpFunc = EasingFunction.GetEasingFunction(config.Ease);
            startlerpTime = GameTimerManager.Instance.GetTimeNow();
        }


        public override void Dispose()
        {
            weatherSystem.RemoveFromMap(Id);
            //base
            Id = default;
            Priority = default;
            IsOver = true;
            Data?.Dispose();
            Data = null;
            //this
            From = null;
            To = null;
            formData?.Dispose();
            formData = null;
            toData?.Dispose();
            toData = null;
            lerpFunc = null;
            config = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}