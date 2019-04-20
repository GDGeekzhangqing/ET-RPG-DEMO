﻿using ETModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ETHotfix
{
    public static class ActiveSkillComponentSystem
    {


        public static async ETVoid Execute(this ActiveSkillComponent self, string skillId, ETTaskCompletionSource<bool> tcs)
        {

            if (self.usingSkill
            || !self.skillList.ContainsKey(skillId)
            || !SkillHelper.CheckIfSkillCanUse(skillId, self.GetParent<Unit>()))
            {
                tcs.SetResult(false);
                return;
            } 
            ActiveSkillData activeSkillData = Game.Scene.GetComponent<SkillConfigComponent>().GetActiveSkill(skillId);
            SkillHelper.ExecuteSkillParams excuteSkillParams = new SkillHelper.ExecuteSkillParams();
            excuteSkillParams.skillId = skillId;
            excuteSkillParams.source = self.GetParent<Unit>();
            excuteSkillParams.skillLevel = 1;
            self.usingSkill = true;
            bool canUse = await SkillHelper.CheckInput(excuteSkillParams);
            self.usingSkill = false;
            self.currUsingSkillId = skillId;
            if (!canUse)
            {
                tcs.SetResult(false);
                return;
            }
            //TODO: 暂时先直接取消之前的行动
            tcs.SetResult(true);
            self.cancelToken?.Cancel();
            Game.EventSystem.Run(EventIdType.CancelPreAction, self.GetParent<Unit>());
            CharacterStateComponent characterStateComponent = self.GetParent<Unit>().GetComponent<CharacterStateComponent>();
            characterStateComponent.Set(SpecialStateType.NotInControl, true);
            self.cancelToken = new CancellationTokenSource();
            excuteSkillParams.cancelToken = self.cancelToken;
            
            await SkillHelper.ExecuteActiveSkill(excuteSkillParams);
            self.cancelToken = null;
            characterStateComponent.Set(SpecialStateType.NotInControl, false);
            self.currUsingSkillId = string.Empty;


        }


        public static void AddSkill(this ActiveSkillComponent self, string skillId)
        {
            ActiveSkillData activeSkillData = Game.Scene.GetComponent<SkillConfigComponent>().GetActiveSkill(skillId);
            if (activeSkillData.isNormalAttack)
            {
                self.Skill_NormalAttack = skillId;
            }
            if (!self.skillList.ContainsKey(skillId))
            {
                self.skillList.Add(skillId, new BaseSkill_AppendedData() { level = 1 });
            }
        }

        public static void RemoveSkill(this ActiveSkillComponent self, string skillId)
        {
            if (!self.skillList.ContainsKey(skillId)) return;
            if (skillId == self.Skill_NormalAttack) return;
            self.skillList.Remove(skillId);
        }

        public static BaseSkill_AppendedData GetSkillAppendedData(this ActiveSkillComponent self, string skillId)
        {
            if (self.skillList.TryGetValue(skillId, out var data))
                return data;
            else
                return null;
        }
    }
}
