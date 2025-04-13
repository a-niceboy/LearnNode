using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace battleEngine
{
    #region 枚举配置等
    // 阵营
    public enum CampType
    {
        Red,
        Blue
    }

    public static class Config
    {
        public const int minAttFrame = 0;
    }

    #endregion

    #region 工具方法
    public static class Util
    {
        private static Random _random;
        public static Random Random
        {
            get
            {
                if (_random == null)
                {
                    _random = new Random(1);
                }
                return _random;
            }
        }

        public static void PerformanceTest()
        {
            const int TEST_COUNT = 10000000;
            var source = new BattleUnit("TestSource", 100, 10, 5, CampType.Red);
            var target = new BattleUnit("TestTarget", 100, 10, 5, CampType.Blue);


            // 真实测试 - New方式
            GC.Collect(); // 确保公平比较
            Console.WriteLine($"GC回收次数：{GC.CollectionCount(0)}");
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < TEST_COUNT; i++)
            {
                var e = new AttackEvent(source, target, 10);
                // 模拟真实使用
                EventManager.Publish(e);
                // 需要手动触发GC回收
                if (i % 10000 == 0) GC.Collect();
            }
            var newTime = sw.ElapsedMilliseconds;

            var pool = EventPool<AttackEvent>.Instance;
            sw.Restart();
            for (int i = 0; i < TEST_COUNT; i++)
            {
                var e = pool.Get();
                {
                    e.Source = source;
                    e.Target = target;
                    e.Damage = 10;
                    EventManager.Publish(e);
                    pool.Release(e);
                }
            }
            var poolTime = sw.ElapsedMilliseconds;

            Console.WriteLine($"测试结果（{TEST_COUNT}次）：");
            Console.WriteLine($"New方式: {newTime}ms");
            Console.WriteLine($"对象池1: {poolTime}ms");
            Console.WriteLine($"GC回收次数：{GC.CollectionCount(0)}");
        }
    }
    #endregion

    #region 日志
    public static class Log
    {
        public static void log(string s)
        {
            Console.WriteLine(s);
        }
    }
    #endregion

    #region 事件
    public abstract class BattleEvent : IPoolableEvent
    {
        public BattleUnit Source;

        public BattleEvent() { }
        public BattleEvent(BattleUnit source)
        {
            Source = source;
        }
        public virtual void Reset()
        {
            Source = null;
        }
    }

    // 攻击事件类型
    public class AttackEvent : BattleEvent
    {
        byte[] _payload = new byte[1024];
        public BattleUnit Target;
        public int Damage;
        public AttackEvent() { }
        public AttackEvent(BattleUnit source, BattleUnit target, int damage)
            :base(source)
        {
            Target = target;
            Damage = damage;
        }

        public override void Reset()
        {
            base.Reset();
            Target = null;
            Damage = 0;
        }
    }

    // 伤害事件类型
    public class DamageEvent : BattleEvent
    {
        public BattleUnit Target;
        public int Damage;
        public bool IsCritical; // 暴击标志
        public bool IsSkillDamage; // 是否来自技能
        public DamageEvent() { }
        public DamageEvent(BattleUnit source, BattleUnit target, int damage, bool isSkillDamage = false)
            : base(source)
        {
            Target = target;
            Damage = damage;
            IsSkillDamage = isSkillDamage;
        }

        public override void Reset()
        {
            base.Reset();
            Target = null;
            Damage = 0;
            IsCritical = false;
            IsSkillDamage = false;
        }
    }

    // 发技能事件类型
    public class SkillCastEvent : BattleEvent
    {
        public BattleUnit Target;
        public SkillData Skill;
        public SkillCastEvent() { }
        public SkillCastEvent(BattleUnit source, SkillData skill, BattleUnit target)
            : base(source)
        {
            Skill = skill;
            Target = target;
        }

        public override void Reset()
        {
            base.Reset();
            Target = null;
            Skill = null;
        }
    }

    // 死亡事件类型
    public class DeathEvent : BattleEvent
    {
        public DeathEvent() { }
        public DeathEvent(BattleUnit source) : base(source) { }
    }

    // Buff 应用事件
    public class BuffApplyEvent : BattleEvent
    {
        public BattleUnit Target;
        public BuffType BuffType;
        public int Value;
        public int DurationFrames;
        public BuffApplyEvent() { }
        public BuffApplyEvent(BattleUnit source, BattleUnit target,
                            BuffType type, int value, int duration)
            : base(source)
        {
            Target = target;
            BuffType = type;
            Value = value;
            DurationFrames = duration;
        }

        public override void Reset()
        {
            base.Reset();
            Target = null;
            BuffType = BuffType.AttackBoost;
            Value = 0;
            DurationFrames = 0;
        }
    }

    // Buff 过期事件
    public class BuffExpiredEvent : BattleEvent
    {
        public BuffType BuffType;
        public BuffExpiredEvent() { }
        public BuffExpiredEvent(BattleUnit target, BuffType type)
            : base(target)
        {
            BuffType = type;
        }

        public override void Reset()
        {
            base.Reset();
        }
    }


    // 战斗结束事件类型
    public class BattleEndEvent : BattleEvent
    {
        public CampType WinningCamp;
        public BattleEndEvent() { }
        public BattleEndEvent(CampType winningCamp) : base(null)
        {
            WinningCamp = winningCamp;
        }
    }

    // 战斗开始事件类型
    public class BattleStartEvent : BattleEvent
    {
        public BattleStartEvent() : base(null)
        {
        }
    }


    #endregion

    #region 事件对象池
    public interface IPoolableEvent
    {
        void Reset(); // 用于重置事件状态
    }

    public class EventPool<T> where T : IPoolableEvent, new()
    {
        private readonly T[] _pool;
        private int _index;
        private readonly Func<T> _factory;

        private static EventPool<T> _instance;
        public static EventPool<T> Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EventPool<T>();

                return _instance;
            }
        }

        private EventPool(int size = 1024)
        {
            _pool = new T[size];
            _factory = () => new T();

            // 预填充对象
            for (int i = 0; i < size; i++)
            {
                _pool[i] = _factory();
            }
            _index = size - 1;
        }
        public T Get()
        {
            return _index >= 0 ? _pool[_index--] : _factory();
        }

        public void Release(T item)
        {
            if (_index < _pool.Length - 1 && item != null) {
                item.Reset();
                _pool[++_index] = item;
            }
        }
    }

    public class PublicEventPool<T> where T : IPoolableEvent, new()
    {
        private readonly T[] _pool;
        private int _index;
        private readonly Func<T> _factory;

        public PublicEventPool(int size = 1024)
        {
            _pool = new T[size];
            _factory = () => new T();

            // 预填充对象
            for (int i = 0; i < size; i++)
            {
                _pool[i] = _factory();
            }
            _index = size - 1;
        }
        public T Get()
        {
            return _index >= 0 ? _pool[_index--] : _factory();
        }

        public void Release(T item)
        {
            if (_index < _pool.Length - 1 && item != null)
            {
                item.Reset();
                _pool[++_index] = item;
            }
        }
    }

    // 使用示例的结构化处理类
    //public struct PooledEvent<T> : System.IDisposable where T : class, IPoolableEvent, new()
    //{
    //    public T Event { get; }

    //    public PooledEvent(T eventObj)
    //    {
    //        Event = eventObj;
    //    }

    //    public void Dispose()
    //    {
    //        EventPool<T>.Instance.Release(Event);
    //    }
    //}

    #endregion

    #region 事件管理
    // 事件管理
    public static class EventManager
    {
        private static readonly Dictionary<Type, List<KeyValuePair<int, Action<BattleEvent>>>> _eventHandlers = new Dictionary<Type, List<KeyValuePair<int, Action<BattleEvent>>>>();

        // 注册监听事件
        public static void Subscribe<T>(Action<T> handler, int priority = 0) where T : BattleEvent
        {
            Type eventType = typeof(T);

            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<KeyValuePair<int, Action<BattleEvent>>>();
            }

            // 允许相同优先级的多个处理器
            _eventHandlers[eventType].Add(new KeyValuePair<int, Action<BattleEvent>>(priority, e => handler((T)e)));

            // 按优先级排序（数值越小优先级越高）
            _eventHandlers[eventType].Sort((a, b) => a.Key.CompareTo(b.Key));
        }

        // 广播事件
        public static void Publish(BattleEvent battleEvent)
        {
            Type eventType = battleEvent.GetType();

            if (_eventHandlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handlerPair in handlers)
                {
                    handlerPair.Value(battleEvent);
                }
            }
        }
    }

    #endregion

    
    #region 技能

    // 技能状态
    public enum SkillState
    {
        Ready,      // 准备好可释放
        Cooldown,   // 倒计时恢复中
        Disabled,   // 不可使用
    }

    public enum SkillTargetType
    {
        Own,
        TeamOne,
        TeamAll,
        EnemyOne,
        EnemyAll,
    }
    public class SkillData
    {
        public string skillName;
        // 在SkillData中添加属性验证
        private int _remainingCooldown;
        public int RemainingCooldown
        {
            get => _remainingCooldown;
            set
            {
                _remainingCooldown = Math.Max(0, value);
                if (_remainingCooldown == 0)
                    state = SkillState.Ready;
            }
        }
        public int cooldownFrames;  // 冷却时间(帧数)
        public int cost;           // 消耗(MP/能量等)

        public SkillTargetType skillTargetType;

        public SkillState state;
        public List<SkillEffect> effects;

        public SkillData()
        {
            RemainingCooldown = 0;
            state = SkillState.Ready;
            effects = new List<SkillEffect>();
        }
    }
    public enum SkillEffectType { Damage, Heal, Buff }
    public struct SkillEffect
    {
        public BuffType buffType;
        public SkillEffectType type;
        public int value;
        public int durationFrames; // 对于持续效果

        public SkillEffect(SkillEffect skillEffect)
        {
            buffType = skillEffect.buffType;
            type = skillEffect.type;
            value = skillEffect.value;
            durationFrames = skillEffect.durationFrames;
        }
    }
    #endregion

    #region 技能管理
    public class SkillManager
    {
        public List<SkillData> skills;
        private BattleUnit unit;

        public void SetUnit(BattleUnit unit)
        {
            this.unit = unit;
        }

        public void SetSkill(SkillData skill)
        {
            if (skills == null)
                skills = new List<SkillData>();

            skills.Add(skill);
        }

        public void Update()
        {
            // 更新所有技能冷却
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill.state == SkillState.Cooldown)
                    {
                        skill.RemainingCooldown--;
                    }
                }
            }
        }

        public bool TryCastSkill(int skillIndex)
        {
            if (skillIndex < 0 || skillIndex >= skills.Count)
                return false;

            var skill = skills[skillIndex];

            // 检查技能是否可用
            if (skill.state != SkillState.Ready)
                return false;

            // 执行技能
            ExecuteSkill(skill);

            // 进入冷却
            skill.state = SkillState.Cooldown;
            skill.RemainingCooldown = skill.cooldownFrames;

            return true;
        }
        public void ExecuteSkill(SkillData skill)
        {
            foreach (var effect in skill.effects)
            {
                switch (effect.type)
                {
                    case SkillEffectType.Damage:
                        // 发布攻击事件而不是直接调用
                        Log.log($"{unit.name} 发起技能 {skill.skillName}");

                        //using (var pooledEvent = new PooledEvent<DamageEvent>(EventPool<DamageEvent>.Instance.Get()))
                        //{
                        //    var damageEvent = pooledEvent.Event;
                        //    damageEvent.Source = unit;
                        //    damageEvent.Target = unit.GetSkillTarget(skill.skillTargetType);
                        //    damageEvent.Damage = effect.value;
                        //    damageEvent.IsSkillDamage = true;
                        //    EventManager.Publish(damageEvent);
                        //}
                        
                        break;
                    case SkillEffectType.Heal:
                        Log.log($"{unit.name} 发起技能 {skill.skillName}");
                        int lastHp = unit.Hp;
                        unit.Hp = Math.Min(unit.Hp + effect.value, unit.MaxHp);
                        Log.log($"{unit.name} 恢复 {effect.value} 生命值 血量{lastHp}->{unit.Hp}");
                        break;
                    case SkillEffectType.Buff:
                        BattleUnit target = unit.GetSkillTarget(skill.skillTargetType);
                        if (target != null)
                        {
                            Log.log($"{unit.name} 发起Buff {skill.skillName}");

                            //using (var pooledEvent = new PooledEvent<BuffApplyEvent>(EventPool<BuffApplyEvent>.Instance.Get()))
                            //{
                            //    var buffApplyEvent = pooledEvent.Event;
                            //    buffApplyEvent.Source = unit;
                            //    buffApplyEvent.Target = target;
                            //    buffApplyEvent.BuffType = effect.buffType;
                            //    buffApplyEvent.Value = effect.value;
                            //    buffApplyEvent.DurationFrames = effect.durationFrames;
                            //    EventManager.Publish(buffApplyEvent);
                            //}
                        }

                        break;
                        // 其他效果类型...
                }
            }
        }
    }

    #endregion

    #region 事件伤害处理系统
    // 创建全局伤害处理系统
    public class DamageCalculationSystem
    {
        public DamageCalculationSystem()
        {
            EventManager.Subscribe<DamageEvent>(OnDamageEvent, priority: -1);
        }

        private void OnDamageEvent(DamageEvent e)
        {
            //Log.log("伤害逻辑集中处理");
            // 攻击buff
            e.Damage += BattleManager.Instance.buffSystem.GetAttackBonus(e.Source);
            // 统一暴击计算
            if (CheckCritical(e.Source))
            {
                e.Damage *= 2;
                e.IsCritical = true;
            }

            // 护甲buff
            int defense = e.Target.defense;
            defense += BattleManager.Instance.buffSystem.GetDefenseBonus(e.Target);
            // 护甲减免
            e.Damage = Math.Max(1, e.Damage - defense);
        }

        private bool CheckCritical(BattleUnit unit)
        {
            return Util.Random.NextDouble() < 0.1f; // 10%暴击率
        }
    }

    #endregion

    #region Buff
    // Buff 类型枚举
    public enum BuffType
    {
        AttackBoost,    // 攻击提升
        DefenseBoost,   // 防御提升
        SpeedBoost,     // 攻速提升
        Poison          // 中毒效果
    }

    public class ActiveBuff
    {
        public BuffType Type { get; }
        public int Value { get; }
        public int RemainingFrames { get; set; }

        public ActiveBuff(BuffApplyEvent e)
        {
            Type = e.BuffType;
            Value = e.Value;
            RemainingFrames = e.DurationFrames;
        }
    }

    public class BuffSystem
    {
        private Dictionary<BattleUnit, List<ActiveBuff>> _activeBuffs = new Dictionary<BattleUnit, List<ActiveBuff>>();

        public BuffSystem()
        {
            EventManager.Subscribe<BuffApplyEvent>(OnBuffApply);
        }

        // Buff 应用处理
        private void OnBuffApply(BuffApplyEvent e)
        {
            if (!_activeBuffs.ContainsKey(e.Target))
            {
                _activeBuffs[e.Target] = new List<ActiveBuff>();
            }

            ActiveBuff exist = null;
            foreach (var item in _activeBuffs[e.Target])
            {
                if (item.Type == e.BuffType)
                {
                    exist = item;
                    break;
                }
            }

            if (exist != null)
            {
                // 刷新持续时间（可改为叠加逻辑）
                exist.RemainingFrames = e.DurationFrames;
            }
            else
            {
                // 添加新 Buff
                _activeBuffs[e.Target].Add(new ActiveBuff(e));
            }

            Log.log($"{e.Target.name} 获得 {e.BuffType} 效果");
        }


        // 获取单位的 Buff 总加成
        public int GetAttackBonus(BattleUnit unit)
        {
            return GetTotalBonus(unit, BuffType.AttackBoost);
        }

        public int GetDefenseBonus(BattleUnit unit)
        {
            return GetTotalBonus(unit, BuffType.DefenseBoost);
        }

        public int GetSpeedBonus(BattleUnit unit)
        {
            return GetTotalBonus(unit, BuffType.SpeedBoost);
        }

        private int GetTotalBonus(BattleUnit unit, BuffType type)
        {
            int val = 0;
            if (_activeBuffs.TryGetValue(unit, out var buffs))
            {
                for (int i = 0; i < buffs.Count; i++)
                {
                    if (buffs[i].Type == type)
                    {
                        val += buffs[i].Value;
                    }
                }
            }
            return val;
        }

        public void Update()
        {
            foreach (var kv in _activeBuffs)
            {
                var unitBuffs = kv.Value;
                var unit = kv.Key;
                for (int i = 0; i < unitBuffs.Count; i++)
                {
                    var unitBuff = unitBuffs[i];
                    unitBuff.RemainingFrames--;
                    if (unitBuff.RemainingFrames <= 0)
                    {
                        unitBuffs.Remove(unitBuff);

                        //using (var pooledEvent = new PooledEvent<BuffExpiredEvent>(EventPool<BuffExpiredEvent>.Instance.Get()))
                        //{
                        //    var buffExpiredEvent = pooledEvent.Event;
                        //    buffExpiredEvent.Source = unit;
                        //    buffExpiredEvent.BuffType = unitBuff.Type;
                        //    EventManager.Publish(buffExpiredEvent);
                        //}
                        Log.log($"{unit.name} 的 {unitBuff.Type} 效果消失");
                    }
                }
            }
        }
    }

    #endregion

    #region 战斗单元
    public class BattleUnit
    {
        public string name;
        public int MaxHp { get; private set; }
        private int _hp;
        public int Hp
        {
            get => _hp;
            set
            {
                if (value <= MaxHp)
                {
                    _hp = value;
                }
            }
        }
        public int defense;
        public CampType camp;
        public int attackFrame;
        public float AttackSpeed => 1f / attackFrame;
        public int attackValue;
        public SkillManager skill;

        private int curFrame;

        public List<BattleUnit> attackedValues;

        public BattleGroup group;

        public BattleUnit(string name, int MaxHp, int attackFrame, int attackValue, CampType camp)
        {
            this.name = name;
            this.MaxHp = MaxHp;
            this.Hp = MaxHp;
            this.attackFrame = attackFrame;
            this.attackValue = attackValue;
            this.defense = 1;
            this.camp = camp;

            curFrame = 0;

            // 注册事件处理
            EventManager.Subscribe<DamageEvent>(OnDamage);
        }

        public void AddSkill(SkillData skillData)
        {
            if (skill == null)
            {
                skill = new SkillManager();
                skill.SetUnit(this);
            }

            skill.SetSkill(skillData);
        }

        public void Update()
        {
            int speedBonus = BattleManager.Instance.buffSystem.GetSpeedBonus(this);
            int actualAttackFrame = Math.Min(Config.minAttFrame, attackFrame - speedBonus);

            curFrame++;
            if (curFrame >= actualAttackFrame)
            {
                curFrame -= actualAttackFrame;
                TryAttack();
            }
            else
            {
                Log.log(name + "攻击cd " +(actualAttackFrame - curFrame));
            }

            if (skill != null)
            {
                skill.Update();
            }
        }

        public void Die()
        {
            Log.log(name + "死亡");
        }

        public BattleUnit GetTarget()
        {
            return group.manager.GetAttackedUnit(this);
        }

        public BattleUnit GetSkillTarget(SkillTargetType skillTargetType)
        {
            return group.manager.GetSkillTarget(this, skillTargetType);
        }
        public bool TryCastSkill(int skillIndex = 0)
        {
            if (skill == null || skill.skills.Count == 0)
                return false;

            return skill.TryCastSkill(skillIndex);
        }

        public bool TrySkill()
        {
            if (skill == null)
            {
                return false;
            }

            for (int i = 0; i < skill.skills.Count; i++)
            {
                if (skill.TryCastSkill(i))
                {
                    return true;
                }
            }

            return false;
        }

        public void TryAttack()
        {
            BattleUnit target = GetTarget();
            if (target != null)
            {
                bool trySkill = TrySkill();
                if (trySkill)
                {
                }
                else
                {
                    // 发布攻击事件而不是直接调用
                    Log.log($"{name} 发起攻击 {target.name}");
                    //using (var pooledEvent = new PooledEvent<AttackEvent>(EventPool<AttackEvent>.Instance.Get()))
                    //{
                    //    var attackEvent = pooledEvent.Event;
                    //    attackEvent.Source = this;
                    //    attackEvent.Target = target;
                    //    attackEvent.Damage = attackValue;
                    //    EventManager.Publish(attackEvent);
                    //}
                }
            }
        }

        private void OnDamage(DamageEvent e)
        {
            if (e.Target == this)
            {
                int _defense = defense;

                _defense += BattleManager.Instance.buffSystem.GetAttackBonus(this);

                int finalDamage = Math.Max(1, e.Damage - _defense);

                string damageSource = e.IsSkillDamage ? "技能" : "攻击";
                string critText = e.IsCritical ? "暴击！" : "";
                Log.log($"{name} 受到{e.Source.name}的{damageSource}{critText} 伤害:{e.Damage} 防御{_defense} 血量:{Hp}->{Hp - finalDamage}");

                ApplyDamage(finalDamage);
            }
        }

        public void ApplyDamage(int amount)
        {
            Hp -= amount;

            if (Hp <= 0)
            {
                Die();

                //using (var pooledEvent = new PooledEvent<DeathEvent>(EventPool<DeathEvent>.Instance.Get()))
                //{
                //    var deathEvent = pooledEvent.Event;
                //    deathEvent.Source = this;
                //    EventManager.Publish(deathEvent);
                //}
            };
        }

    }


    #endregion

    #region 战斗组
    public class BattleGroup
    {
        public List<BattleUnit> battleUnits;
        public BattleManager manager;
        public CampType camp;

        public BattleGroup(CampType camp, BattleManager manager)
        {
            this.camp = camp;
            this.manager = manager;
            battleUnits = new List<BattleUnit>();
        }

        public void AddUnit(BattleUnit unit)
        {
            battleUnits.Add(unit);
            unit.group = this;
        }

        public void Update()
        {
            for (int i = 0; i < battleUnits.Count; i++)
            {
                battleUnits[i].Update();
            }
        }

        public bool IsEmpty()
        {
            return battleUnits.Count == 0;
        }
    }

    #endregion

    #region 战斗管理
    public class BattleManager
    {
        private static BattleManager instance;

        public BattleGroup battleGroupRed;
        public BattleGroup battleGroupBlue;
        public BuffSystem buffSystem;
        public DamageCalculationSystem damageCalculationSystem;

        public bool log = false;
        public bool isRun = false;

        private BattleManager()
        {
            battleGroupBlue = new BattleGroup(CampType.Blue, this);
            battleGroupRed = new BattleGroup(CampType.Red, this);
            buffSystem = new BuffSystem();
            damageCalculationSystem = new DamageCalculationSystem();

            // 注册事件处理
            EventManager.Subscribe<AttackEvent>(OnAttack);
            EventManager.Subscribe<DeathEvent>(OnDeath);
        }

        public static BattleManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new BattleManager();

                return instance;
            }
        }

        private void OnAttack(AttackEvent e)
        {
            bool dodge = CheckDodge(e.Target);
            if (dodge)
            {
                Log.log($"{e.Target.name}闪避了！{e.Source.name}的攻击");
                return;
            }

            // 将攻击事件转换为伤害事件 伤害结果统一优先事件处理
            //using (var pooledEvent = new PooledEvent<DamageEvent>(EventPool<DamageEvent>.Instance.Get()))
            //{
            //    var damageEvent = pooledEvent.Event;
            //    damageEvent.Source = e.Source;
            //    damageEvent.Target = e.Target;
            //    damageEvent.Damage = e.Damage;
            //    EventManager.Publish(damageEvent);
            //}
        }

        private bool CheckDodge(BattleUnit unit)
        {
            return Util.Random.NextDouble() < 0.1f; // 10%回避率
        }

        private void OnDeath(DeathEvent e)
        {
            e.Source.group.battleUnits.Remove(e.Source);
            Log.log($"{e.Source.name}移除战斗");

            // 检查战斗是否结束
            if (battleGroupBlue.IsEmpty())
            {
                Finish(CampType.Red);

                //using (var pooledEvent = new PooledEvent<BattleEndEvent>(EventPool<BattleEndEvent>.Instance.Get()))
                //{
                //    var battleEndEvent = pooledEvent.Event;
                //    battleEndEvent.WinningCamp = CampType.Red;
                //    EventManager.Publish(battleEndEvent);
                //}
            }
            else if (battleGroupRed.IsEmpty())
            {
                Finish(CampType.Red);
                //using (var pooledEvent = new PooledEvent<BattleEndEvent>(EventPool<BattleEndEvent>.Instance.Get()))
                //{
                //    var battleEndEvent = pooledEvent.Event;
                //    battleEndEvent.WinningCamp = CampType.Blue;
                //    EventManager.Publish(battleEndEvent);
                //}
            }
        }


        public void Start()
        {
            //using (var pooledEvent = new PooledEvent<BattleStartEvent>(EventPool<BattleStartEvent>.Instance.Get()))
            //{
            //    var battleStartEvent = pooledEvent.Event;
            //    EventManager.Publish(battleStartEvent);
            //}
            isRun = true;
            while (isRun)
            {
                Update();
                Thread.Sleep(16); // 约60FPS
            }
        }

        public void Update()
        {
            battleGroupBlue.Update();
            battleGroupRed.Update();
            buffSystem.Update();
        }

        public void AddUnit(BattleUnit unit)
        {
            if (unit.camp == CampType.Blue)
            {
                battleGroupBlue.AddUnit(unit);
            }

            if (unit.camp == CampType.Red)
            {
                battleGroupRed.AddUnit(unit);
            }
        }

        public ITargetSelector targetSelector = new LowestHpTargetSelector();
        public BattleUnit GetAttackedUnit(BattleUnit attacker)
        {
            var enemies = attacker.camp == CampType.Blue
            ? battleGroupRed.battleUnits
            : battleGroupBlue.battleUnits;
            return targetSelector.SelectTarget(attacker, enemies);
        }

        public void Finish(CampType campType)
        {
            Log.log("结束"+ campType + "胜利");
            isRun = false;
        }

        internal BattleUnit GetSkillTarget(BattleUnit battleUnit, SkillTargetType skillTargetType)
        {
            BattleUnit ret = null;
            switch (skillTargetType)
            {
                case SkillTargetType.Own:
                    ret = battleUnit;
                    break;
                case SkillTargetType.TeamOne:
                    var team = battleUnit.camp == CampType.Red
                    ? battleGroupRed.battleUnits
                    : battleGroupBlue.battleUnits;
                    if(team.Count > 0)
                    {
                        for (int i = 0; i < team.Count; i++)
                        {
                            if(team[i]!= battleUnit)
                            {
                                ret = team[i];
                                break;
                            }
                        }
                    }
                    break;
                case SkillTargetType.TeamAll:
                    // 未处理
                    break;
                case SkillTargetType.EnemyOne:
                    ret = GetAttackedUnit(battleUnit);
                    break;
                case SkillTargetType.EnemyAll:
                    break;
                default:
                    break;
            }
            return ret;
        }
    }

    #endregion

    #region 目标筛选
    // 实现不同策略
    public interface ITargetSelector
    {
        BattleUnit SelectTarget(BattleUnit source, List<BattleUnit> candidates);
    }

    public class FirstTargetSelector : ITargetSelector
    {
        public BattleUnit SelectTarget(BattleUnit source, List<BattleUnit> candidates)
        {
            BattleUnit unit = null;
            if (candidates.Count > 0)
            {
                unit = candidates[0];
            }
            return unit;
        }
    }
    public class LowestHpTargetSelector : ITargetSelector
    {
        public BattleUnit SelectTarget(BattleUnit source, List<BattleUnit> candidates)
        {
            BattleUnit unit = null;
            if (candidates.Count > 0)
            {
                unit = candidates[0];
                for (int i = 1; i < candidates.Count; i++)
                {
                    if (unit.Hp > candidates[i].Hp)
                    {
                        unit = candidates[i];
                    }
                }
            }
            return unit;
        }
    }
    #endregion


    #region 入口
    class Program
    {
        static void Main(string[] args)
        {
            BattleUnit redUnit = new BattleUnit("红1", 20, 5, 5, CampType.Red);
            SkillData fireball = new SkillData
            {
                skillName = "火球术",
                cooldownFrames = 15,
                cost = 10,
                effects = new List<SkillEffect> {
                    new SkillEffect {
                        type = SkillEffectType.Damage,
                        value = 10,
                    }
                }
            };
            SkillData heal = new SkillData
            {
                skillName = "回血术",
                cooldownFrames = 15,
                cost = 10,
                effects = new List<SkillEffect> {
                    new SkillEffect {
                        type = SkillEffectType.Heal,
                        value = 10,
                    }
                }
            };
            SkillData addAttSpeedBuff = new SkillData
            {
                skillName = "加攻速",
                cooldownFrames = 5,
                cost = 10,
                effects = new List<SkillEffect> {
                    new SkillEffect {
                        type = SkillEffectType.Buff,
                        buffType = BuffType.SpeedBoost,
                        durationFrames = 10,
                        value = 4,
                    }
                }
            };

            redUnit.AddSkill(fireball);
            redUnit.AddSkill(heal);
            redUnit.AddSkill(addAttSpeedBuff);

            BattleUnit blueUnit = new BattleUnit("蓝1", 20, 2, 2, CampType.Blue);
            BattleUnit blueUnit2 = new BattleUnit("蓝2", 20, 2, 2, CampType.Blue);

            //BattleManager.Instance.AddUnit(redUnit);
            //BattleManager.Instance.AddUnit(blueUnit);
            //BattleManager.Instance.AddUnit(blueUnit2);

            //BattleManager.Instance.Start();

            Util.PerformanceTest();

            Console.ReadLine();
        }
    }
    #endregion
}
