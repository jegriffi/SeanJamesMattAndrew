using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillFactory
{
    private static Skill bloodDonor = null;
    public static Skill GetBloodDonor()
    {
        if (bloodDonor == null)
        {
            bloodDonor = new Skill();
            bloodDonor.name = "Blood\nDonor";
            bloodDonor.icon = Resources.Load<Sprite>("SpellIcons/bloodDonor");

            bloodDonor.basePower = 3;
            bloodDonor.aoe = 0;
            bloodDonor.range = 4;
            bloodDonor.manaCost = user => 3;
            bloodDonor.cooldown = 3;
            bloodDonor.damageType = Skill.DamageType.DAMAGE;
            bloodDonor.targetType = Skill.TargetType.ENEMY;
            bloodDonor.OnTarget = (user, target, args) =>
            {
                float amt = user.DamageMultiplier * bloodDonor.basePower * (1f - target.Armor);
                target.TakeDamage(amt, user);
                user.TakeDamage(-amt, user);
            };
            bloodDonor.GenerateTasks = (user, tile, args) =>
            {
                List<Unit> list = bloodDonor.gatherTargets(user, tile);
                foreach (Unit target in list) {
                    GameManager.instance.tasks.Add(new Task_Face_Eachother(user, target));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                    GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                    GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, target.tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube")));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(target, "Hit"));
                }
                bloodDonor.EnqueueExecuteTask(user, tile, args);
            };
        }
        return bloodDonor;
    }

    //MEDIC
    private static Skill heal = null;
    public static Skill GetHeal() {
        if (heal == null) {
            heal = new Skill();
            heal.name = "Heal";
            heal.icon = Resources.Load<Sprite>("SkillIcons/heal");

            heal.basePower = 2;
            heal.aoe = 0;
            heal.range = 6;
            heal.manaCost = user => 1;
            heal.cooldown = 1;
            heal.damageType = Skill.DamageType.HEAL;
            heal.targetType = Skill.TargetType.ALLY;
            heal.OnTarget = (user, target, args) => {

                target.TakeDamage((-1 * user.DamageMultiplier * heal.basePower), user);
                if (user.talentTags.Contains("Balance"))
                {
                    target.AddEffect(EffectFactory.getBalanceBuffEffect(), 3);
                    Unit enemy = GameManager.instance.GetNearestEnemy(target);
                    if (enemy != null)
                    {
                        enemy.TakeDamage(heal.basePower * user.DamageMultiplier * (1f - enemy.Armor), user);
                        enemy.AddEffect(EffectFactory.getBalanceDebuffEffect(), 3);
                    }
                }

            };
            heal.GenerateTasks = (user, tile, args) => {
                foreach (Unit target in heal.gatherTargets(user, tile)) {
                    GameManager.instance.tasks.Add(new Task_Face_Eachother(user, target));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                    GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                    GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, target.tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube")));
                    GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/MEDIC/HEAL/heal prefab"), target.transform.position + Vector3.up, 1));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(target, "Hit"));
                }
                heal.EnqueueExecuteTask(user, tile, args);
            };
        }
        return heal;
    }

    private static Skill aoeHeal = null;
    public static Skill GetAoEHeal() {
        if (aoeHeal == null) {
            aoeHeal = new Skill();
            aoeHeal.name = "AoE\nHeal";
            aoeHeal.icon = Resources.Load<Sprite>("SpellIcons/aoeHeal");

            aoeHeal.basePower = 3;
            aoeHeal.aoe = 3;
            aoeHeal.range = 4;
            aoeHeal.manaCost = user => 6;
            aoeHeal.cooldown = 2;
            aoeHeal.damageType = Skill.DamageType.HEAL;
            aoeHeal.targetType = Skill.TargetType.CONDITIONAL;
            aoeHeal.targetTypeFunction = user =>
            {
                return user.talentTags.Contains("OathBreaker") ? Skill.TargetType.ENEMY : Skill.TargetType.ALLY;
            };
            aoeHeal.OnTarget = (user, target, args) => {
                if (user.talentTags.Contains("OathBreaker"))
                    target.TakeDamage(DefaultDamageFormula(user, target, aoeHeal), user);
                else
                    target.TakeDamage((-user.DamageMultiplier * aoeHeal.basePower), user);
            };
            aoeHeal.GenerateTasks = (user, tile, args) => {
                GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/MEDIC/AOE HEAL/aoe healer prefab"), user.transform.position + Vector3.up, 1));

                foreach (Unit target in aoeHeal.gatherTargets(user, tile)) {
                    GameManager.instance.tasks.Add(new Task_Face_Eachother(user, target));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                    GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                    //GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position+Vector3.up,target.tile.transform.position+Vector3.up,(GameObject)Resources.Load("SpellVisuals/BulletCube")));
                    GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/MEDIC/AOE HEAL/aoe receiver prefab"), target.transform.position + Vector3.up, 1));
                    //GameManager.instance.tasks.Add(new Task_Trigger_Animation(target,"Hit"));
                }
                aoeHeal.EnqueueExecuteTask(user, tile, args);
            };
        }
        return aoeHeal;
    }

    private static Skill weakenOffense = null;
    public static Skill GetWeakenOffense() {
        if (weakenOffense == null) {
            weakenOffense = new Skill();
            weakenOffense.name = "Weaken\nOffense";
            weakenOffense.icon = Resources.Load<Sprite>("SpellIcons/weakenOffense");

            weakenOffense.basePower = 1;
            weakenOffense.aoe = 0;
            weakenOffense.range = 5;
            weakenOffense.manaCost = user => 1;
            weakenOffense.cooldown = 0;
            weakenOffense.damageType = Skill.DamageType.DAMAGE;
            weakenOffense.targetType = Skill.TargetType.ENEMY;
            weakenOffense.OnTarget = (user, target, args) => {
                //TODO: wait for Sean
                target.AddEffect(EffectFactory.GetWeakenOffenseEffect(), 3);
            };
            weakenOffense.GenerateTasks = (user, tile, args) => {
                foreach (Unit target in weakenOffense.gatherTargets(user, tile)) {
                    GameManager.instance.tasks.Add(new Task_Face_Eachother(user, target));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                    GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                    GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, target.tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube")));
                    GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/MEDIC/WEAKEN OFFENSE/WeakenOffense prefab"), target.transform.position + Vector3.up, 1));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(target, "Hit"));
                }
                weakenOffense.EnqueueExecuteTask(user, tile, args);
            };
        }
        return weakenOffense;
    }


    //TANK
    private static Skill slam = null;
    public static Skill GetSlam()
    {
        if (slam == null)
        {
            slam = new Skill();
            slam.name = "Slam";
            slam.icon = Resources.Load<Sprite>("SpellIcons/slam");

            slam.basePower = 2;
            slam.manaCost = user => 2;
            slam.aoe = 1;
            slam.range = 0;
            slam.cooldown = 2;
            slam.damageType = Skill.DamageType.DAMAGE;
            slam.targetType = Skill.TargetType.ENEMY;
            slam.OnTarget = (user, target, args) => {
                bool flag = user.talentTags.Contains("HardSlam");

                float amt = user.DamageMultiplier * slam.basePower * (1f - target.Armor);
                target.TakeDamage(amt * (flag ? 2 : 1), user);
                target.AddEffect(EffectFactory.getStunEffect(), flag ? 1 : 2);
            };
            slam.GenerateTasks = (user, tile, args) =>
             {
                 GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                 GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                 GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/TANK/SLAM/SlamPrefab"), user.transform.position, 4));
                 foreach (Unit target in slam.gatherTargets(user, tile))
                 {
                     GameManager.instance.tasks.Add(new Task_Trigger_Animation(target, "Hit"));
                 }
                 slam.EnqueueExecuteTask(user, tile, args);
             };
        }
        return slam;
    }


    private static Skill weakenDefense = null;
    public static Skill GetWeakenDefense() {
        if (weakenDefense == null) {
            weakenDefense = new Skill();
            weakenDefense.name = "Weakend\nDefense";
            weakenDefense.icon = Resources.Load<Sprite>("SpellIcons/weakenDefense");

            weakenDefense.basePower = 2;
            weakenDefense.manaCost = user => 1;
            weakenDefense.aoe = 1;
            weakenDefense.range = 4;
            weakenDefense.cooldown = 0;
            weakenDefense.damageType = Skill.DamageType.DAMAGE;
            weakenDefense.targetType = Skill.TargetType.ENEMY;
            weakenDefense.OnTarget = (user, target, args) => {
                target.AddEffect(EffectFactory.GetWeakenDefenseEffect(), -1);

            };
            weakenDefense.GenerateTasks = (user, tile, args) => {
                foreach (Unit target in weakenDefense.gatherTargets(user, tile)) {
                    GameManager.instance.tasks.Add(new Task_Face_Eachother(user, target));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                    GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                    GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, target.tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube")));
                    GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/TANK/WEAKEN DEFENSE/WeakenDefense prefab"), target.transform.position, 1));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(target, "Hit"));
                }
                weakenDefense.EnqueueExecuteTask(user, tile, args);
            };
        }
        return weakenDefense;
    }

    private static Skill taunt = null;
    public static Skill GetTaunt() {
        if (taunt == null) {
            taunt = new Skill();
            taunt.name = "Taunt";
            taunt.icon = Resources.Load<Sprite>("SpellIcons/taunt");

            taunt.basePower = 0;
            taunt.aoe = 0;
            taunt.range = 0;
            taunt.manaCost = user => 2;
            taunt.cooldown = 2;
            taunt.damageType = Skill.DamageType.CONDITIONAL;
            taunt.targetType = Skill.TargetType.NONE;
            taunt.OnTarget = (user, target, args) => {
                //user.AddEffect(EffectFactory.getTauntEffect(), 4);
            };
            taunt.GenerateTasks = (user, tile, args) => {
                GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                //TODO: MAKE ANIMATION LAST [SET AMOUNT] TURNS!
                GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/TANK/TAUNT/taunt prefab"), user.transform.position, 1));
                taunt.EnqueueExecuteTask(user, tile, args);
            };
        }
        return taunt;
    }

    private static Skill persistence = null;
    public static Skill GetPersistence()
    {
        if (persistence == null)
        {
            persistence = new Skill();
            persistence.name = "Persistence";
            persistence.icon = Resources.Load<Sprite>("SpellIcons/persistence");

            persistence.basePower = 0;
            persistence.aoe = 0;
            persistence.range = 0;
            persistence.manaCost = user => 2;
            persistence.cooldown = 4;
            persistence.damageType = Skill.DamageType.HEAL;
            persistence.targetType = Skill.TargetType.SELF;
            persistence.OnTarget = (user, target, args) =>
            {
                //user.AddEffect(EffectFactory.getPersistenceEffect(), 6);
            };
            persistence.GenerateTasks = (user, tile, args) =>
            {
                GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/TANK/PERSISTENCE/persistence prefab"), user.transform.position, 1));
                persistence.EnqueueExecuteTask(user, tile, args);
            };
        }
        return persistence;
    }

    //---------------//
    //Assassin Skills//
    //---------------//

    private static Skill shiv = null;
    public static Skill GetShiv()
    {
        if (shiv == null)
        {
            shiv = new Skill();
            shiv.name = "Shiv";
            shiv.icon = Resources.Load<Sprite>("SpellIcons/shiv");

            shiv.basePower = 4;
            shiv.aoe = 0;
            shiv.range = 1;
            shiv.manaCost = user => 0;
            shiv.cooldown = 0;
            shiv.damageType = Skill.DamageType.DAMAGE;
            shiv.targetType = Skill.TargetType.ENEMY;
            shiv.OnTarget = (user, target, args) =>
            {
                float amt = user.DamageMultiplier * shiv.basePower * (1f - target.Armor);
                target.TakeDamage(amt, user);

            };
            shiv.GenerateTasks = (user, tile, args) =>
            {
                List<Unit> list = shiv.gatherTargets(user, tile);
                foreach (Unit target in list) {
                    GameManager.instance.tasks.Add(new Task_Face_Eachother(user, target));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                    GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                    GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/ASSASSIN/SHIV/Shiv prefab"), target.transform.position, 1));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(target, "Hit"));
                }
                shiv.EnqueueExecuteTask(user, tile, args);
            };
        }
        return shiv;
    }


    private static Skill fade = null;
    public static Skill GetFade()
    {
        if (fade == null)
        {
            fade = new Skill();
            fade.name = "Fade";
            fade.icon = Resources.Load<Sprite>("SpellIcons/fade");

            fade.basePower = 0;
            fade.manaCost = user => 1;
            fade.aoe = 0;
            fade.range = 0;
            fade.cooldown = 6;
            fade.damageType = Skill.DamageType.HEAL;
            fade.targetType = Skill.TargetType.SELF;
            fade.OnTarget = (user, target, args) => {
                //bool flag = user.talentTags.Contains("Fade Upgrade");
                target.AddEffect(EffectFactory.getInvincibleEffect(), 3);
                target.AddEffect(EffectFactory.getSmokeEffect(), 3);
            };
            fade.GenerateTasks = (user, tile, args) =>
            {
                List<Unit> list = shiv.gatherTargets(user, tile);
                foreach (Unit target in list)
                {
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                    GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                    GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, target.tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube")));
                    GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/ASSASSIN/FADE/Fade Prefab"), target.transform.position, 1));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(target, "Hit"));
                }
                fade.EnqueueExecuteTask(user, tile, args);
            };
        }
        return fade;
    }


    private static Skill cripple = null;
    public static Skill GetCripple()
    {
        if (cripple == null)
        {
            cripple = new Skill();
            cripple.name = "Cripple";
            cripple.icon = Resources.Load<Sprite>("SpellIcons/cripple");

            cripple.basePower = 1;
            cripple.aoe = 0;
            cripple.range = 4;
            cripple.manaCost = user => 3;
            cripple.cooldown = 0;
            cripple.damageType = Skill.DamageType.DAMAGE;
            cripple.targetType = Skill.TargetType.ENEMY;
            cripple.OnTarget = (user, target, args) =>
            {
                float amt = user.DamageMultiplier * cripple.basePower * (1f - target.Armor);
                target.TakeDamage(amt, user);
                target.AddEffect(EffectFactory.getCrippleEffect(), 3);
            };
            cripple.GenerateTasks = (user, tile, args) =>
            {
                List<Unit> list = cripple.gatherTargets(user, tile);
                foreach (Unit target in list) {
                    GameManager.instance.tasks.Add(new Task_Face_Eachother(user, target));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                    GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                    GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, target.tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube"), 1.2f));
                    GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/ASSASSIN/CRIPPLE/Cripple prefab"), target.transform.position, 1));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(target, "Hit"));
                }
                cripple.EnqueueExecuteTask(user, tile, args);
            };
        }
        return cripple;
    }


    private static Skill epidemic = null;
    public static Skill GetEpidemic()
    {
        if (epidemic == null)
        {
            epidemic = new Skill();
            epidemic.name = "Epidemic";
            epidemic.icon = Resources.Load<Sprite>("SpellIcons/epidemic");

            epidemic.basePower = 8;
            epidemic.aoe = 2;
            epidemic.range = 4;
            epidemic.manaCost = user => 8;
            epidemic.cooldown = 0;
            epidemic.damageType = Skill.DamageType.DAMAGE;
            epidemic.targetType = Skill.TargetType.ENEMY;
            epidemic.OnTarget = (user, target, args) =>
            {
                float amt = user.DamageMultiplier * epidemic.basePower * (1f - target.Armor);
                target.TakeDamage(amt, user);
                //TODO: EPIDEMIC effect\
                //target.AddEffect(EffectFactory.getEpidemicEffect(), 2);
                //maybe add ShowParticleAnimation here? 
            };
            epidemic.GenerateTasks = (user, tile, args) =>
            {
                List<Unit> list = epidemic.gatherTargets(user, tile);
                foreach (Unit target in list)
                {
                    GameManager.instance.tasks.Add(new Task_Face_Eachother(user, target));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                    GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                    GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, target.tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube"), 1.2f));
                    GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/ASSASSIN/EPIDEMIC/epidemic helper prefab"), target.transform.position, 2));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(target, "Hit"));
                }
                epidemic.EnqueueExecuteTask(user, tile, args);
            };
        }
        return epidemic;
    }


    //------------//
    // Tech Skills//
    //------------//
    private static Skill snipe = null;
    public static Skill GetSnipe()
    {
        if (snipe == null)
        {
            snipe = new Skill();
            snipe.name = "Snipe";
            snipe.icon = Resources.Load<Sprite>("SpellIcons/snipe");

            snipe.basePower = 3;
            snipe.aoe = 0;
            snipe.range = 4;
            snipe.manaCost = user => user.talentTags.Contains("FreeSnipe") ? 0 : 2;
            snipe.cooldown = 0;
            snipe.damageType = Skill.DamageType.DAMAGE;
            snipe.targetType = Skill.TargetType.ENEMY;
            snipe.OnTarget = (user, target, args) =>
            {
                float amt = user.DamageMultiplier * snipe.basePower * (1f - target.Armor);
                target.TakeDamage(amt, user);

            };
            snipe.GenerateTasks = (user, tile, args) =>
            {
                List<Unit> list = snipe.gatherTargets(user, tile);
                foreach (Unit target in list) {
                    GameManager.instance.tasks.Add(new Task_Face_Eachother(user, target));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                    GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                    GameManager.instance.tasks.Add(new Task_PlaySound(Resources.Load<AudioClip>("SE/Gun1")));
                    GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, target.tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletSniper"), 9));
                    GameManager.instance.tasks.Add(new Task_PlaySound(Resources.Load<AudioClip>("SE/Thunder4")));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(target, "Hit"));
                    GameManager.instance.tasks.Add(new Task_ShowParticleAnimation((GameObject)Resources.Load("SpellVisuals/TECH/SNIPE/snipe prefab"), target.transform.position + Vector3.up, 1));
                }
                snipe.EnqueueExecuteTask(user, tile, args);
            };
        }
        return snipe;
    }

    private static Skill repair = null;
    public static Skill GetRepair()
    {
        if (repair == null)
        {
            repair = new Skill();
            repair.name = "Repair";
            repair.icon = Resources.Load<Sprite>("SpellIcons/repair");

            repair.basePower = 2;
            repair.aoe = 0;
            repair.range = 4;
            repair.manaCost = user => 2;
            repair.cooldown = 0;
            repair.damageType = Skill.DamageType.HEAL;
            repair.targetType = Skill.TargetType.ALLY;
            repair.OnTarget = (user, target, args) =>
            {
                float amt = -user.DamageMultiplier * repair.basePower;
                target.TakeDamage(amt, user);
                target.AddEffect(EffectFactory.getDamageMultiplierEffect(), 3);
                target.AddEffect(EffectFactory.getWrenchExplosionEffect(), 3);
                target.AddEffect(EffectFactory.getWrenchBuffEffect(), 3);
            };
            repair.GenerateTasks = (user, tile, args) =>
            {
                List<Unit> list = repair.gatherTargets(user, tile);
                foreach (Unit target in list) {
                    GameManager.instance.tasks.Add(new Task_Face_Eachother(user, target));
                    GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                    //GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                    GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, target.tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/Wrench/WrenchProjectile"), 2.0f));
                    //GameManager.instance.tasks.Add(new Task_Trigger_Animation(target,"Hit"));
                }
                repair.EnqueueExecuteTask(user, tile, args);
            };
        }
        return repair;
    }

    private static Skill makeSentry = null;
    public static Skill GetMakeSentry()
    {

        if (makeSentry == null)
        {
            makeSentry = new Skill();
            makeSentry.name = "Sentry";
            makeSentry.icon = Resources.Load<Sprite>("SpellIcons/repair");
            makeSentry.manaCost = user => 3;
            makeSentry.targetType = Skill.TargetType.NONE;
            makeSentry.tileRestriction = Skill.TargetTileRestriction.EMPTY;
            makeSentry.cooldown = 5;
            makeSentry.range = 2;
            makeSentry.OnTilePostEffects += (user, tile, args) =>
            {
                GameObject go = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("AssassinModel"), tile.transform.position, user.transform.rotation);
                Unit u = go.AddComponent<Unit>();
                u.dontPlace = true;
                tile.SetUnit(u);
                u.faction = user.faction;
                u.aiControlled = true;
                u.baseMoveRange = 0;
                u.maxMP = 10;
                u.maxHP = 10;
                u.AddSkill(SkillFactory.GetSnipe());
                u.AddEffect(EffectFactory.getDoomEffect(), 5);
            };
            makeSentry.GenerateTasks = (user, tile, args) =>
            {
                GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube")));
                makeSentry.EnqueueExecuteTask(user, tile, args);
            };
        }
        return makeSentry;
    }

    private static float DefaultDamageFormula(Unit user, Unit target, Skill skill)
    {
        return user.DamageMultiplier * skill.basePower * (1f - target.Armor);
    }

    private static Skill lethalInjection;
    public static Skill getLethalInjection()
    {
        if(lethalInjection  == null)
        {
            lethalInjection = new Skill();
            lethalInjection.name = "Lethal\nInjection";
            lethalInjection.icon = Resources.Load<Sprite>("SpellIcons/shiv");
            lethalInjection.cooldown = 1;
            lethalInjection.manaCost = user => 1;
            lethalInjection.basePower = 2.5f;
            lethalInjection.targetType = Skill.TargetType.ENEMY;
            lethalInjection.range = 3;
            lethalInjection.aoe = 0;
            lethalInjection.OnTarget = (user, target, args) =>
            {
                float amt = DefaultDamageFormula(user, target, lethalInjection);
                target.TakeDamage(amt, user);
                target.AddEffect(EffectFactory.getLethalInjectionEffect(), 2);
            };
        }
        return lethalInjection;
    }

    private static Skill makeMedStation = null;
    public static Skill getMakeMedStation()
    {

        if (makeMedStation == null)
        {
            makeMedStation = new Skill();
            makeMedStation.name = "Med\nStation";
            makeMedStation.icon = Resources.Load<Sprite>("SpellIcons/repair");
            makeMedStation.manaCost = user => 3;
            makeMedStation.targetType = Skill.TargetType.NONE;
            makeMedStation.tileRestriction = Skill.TargetTileRestriction.EMPTY;
            makeMedStation.cooldown = 5;
            makeMedStation.range = 2;
            makeMedStation.OnTilePostEffects += (user, tile, args) =>
            {
                GameObject go = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("MedicModel"), tile.transform.position, user.transform.rotation);
                Unit u = go.AddComponent<Unit>();
                u.dontPlace = true;
                tile.SetUnit(u);
                u.faction = user.faction;
                u.aiControlled = true;
                u.baseMoveRange = 0;
                u.maxMP = 10;
                u.maxHP = 10;
                u.AddEffect(EffectFactory.getMedStationEffect(), -1);
                u.AddEffect(EffectFactory.getDoomEffect(), 5);
            };
            makeMedStation.GenerateTasks = (user, tile, args) =>
            {
                GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube")));
                makeMedStation.EnqueueExecuteTask(user, tile, args);
            };
        }
        return makeMedStation;
    }

    private static Skill makeBattlePet = null;
    public static Skill getMakeBattlePet()
    {

        if (makeBattlePet == null)
        {
            makeBattlePet = new Skill();
            makeBattlePet.name = "Battle\nBot";
            makeBattlePet.icon = Resources.Load<Sprite>("SpellIcons/repair");
            makeBattlePet.manaCost = user => 3;
            makeBattlePet.targetType = Skill.TargetType.NONE;
            makeBattlePet.tileRestriction = Skill.TargetTileRestriction.EMPTY;
            makeBattlePet.cooldown = 5;
            makeBattlePet.range = 2;
            makeBattlePet.OnTilePostEffects += (user, tile, args) =>
            {
                GameObject go = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("basemodel"), tile.transform.position, user.transform.rotation);
                Unit u = go.AddComponent<Unit>();
                u.dontPlace = true;
                tile.SetUnit(u);
                u.faction = user.faction;
                u.aiControlled = true;
                u.baseMoveRange = 2;
                u.maxMP = 10;
                u.maxHP = 10;
                u.baseArmor = 0.2f;
                u.AddSkill(SkillFactory.GetShiv());
                u.AddSkill(SkillFactory.GetSnipe());
                u.AddEffect(EffectFactory.getDoomEffect(), 5);
            };
            makeBattlePet.GenerateTasks = (user, tile, args) =>
            {
                GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube")));
                makeBattlePet.EnqueueExecuteTask(user, tile, args);
            };
        }
        return makeBattlePet;
    }

    private static Skill anatomy;
    public static Skill getAnatomy()
    {
        if(anatomy == null)
        {
            anatomy = new Skill();
            anatomy.name = "Anatomy";
            anatomy.icon = Resources.Load<Sprite>("SpellIcons/fade");
            anatomy.targetType = Skill.TargetType.ALLY;
            anatomy.range = 3;
            anatomy.aoe = 0;
            anatomy.manaCost = user => 3;
            anatomy.OnTarget = (user, target, args) =>
            {
                target.AddEffect(EffectFactory.getAnatomyEffect(), 3);
            };
            anatomy.GenerateTasks = (user, tile, args) =>
            {
                GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube"),3));
                anatomy.EnqueueExecuteTask(user, tile, args);
            };
        }
        return anatomy;
    }

    private static Skill stickyGrenade;
    public static Skill getStickyGrenade()
    {
        if(stickyGrenade == null)
        {
            stickyGrenade = new Skill();
            stickyGrenade.name = "Sticky\nGrenade";
            stickyGrenade.cooldown = 3;
            stickyGrenade.manaCost = user => 4;
            stickyGrenade.range = 3;
            stickyGrenade.OnTarget = (user, target, args) =>
            {
                target.AddEffect(EffectFactory.getStickyGrenadeEffect(), 2);
            };
            stickyGrenade.GenerateTasks = (user, tile, args) =>
            {
                GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube"), 3));
                stickyGrenade.EnqueueExecuteTask(user, tile, args);
            };
        }
        return stickyGrenade;
    }


    private static Skill shadowDrift;
    public static Skill getShadowDrift()
    {
        if(shadowDrift == null)
        {
            shadowDrift = new Skill();
            shadowDrift.name = "Shadow\nDrift";
            shadowDrift.range = 5;
            shadowDrift.targetType = Skill.TargetType.NONE;
            shadowDrift.tileRestriction = Skill.TargetTileRestriction.EMPTY;
            shadowDrift.cooldown = 3;
            shadowDrift.manaCost = user => 3;
            shadowDrift.OnTilePostEffects += (user, tile, args) =>
            {
                user.transform.position = tile.transform.position;
                tile.SetUnit(user);
            };
            shadowDrift.GenerateTasks = (user, tile, args) =>
            {
                GameManager.instance.tasks.Add(new Task_Trigger_Animation(user, "Punch"));
                GameManager.instance.tasks.Add(new Task_Wait(0.3f));
                GameManager.instance.tasks.Add(new Task_Fire_Projectile(user.transform.position + Vector3.up, tile.transform.position + Vector3.up, (GameObject)Resources.Load("SpellVisuals/BulletCube"), 3));
                shadowDrift.EnqueueExecuteTask(user, tile, args);
            };
        }
        return shadowDrift;
    }
}