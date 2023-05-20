namespace Reflyem

open Supporter

open ExtEvent
module MagickaWeaponPlugin =

  let init() =
    if RefConfig.MagickaWeaponSpellToCast.IsNone || RefConfig.MagickaWeaponDamageKeyword.IsNone then
      Log "MagickaWeaponSpellToCast or MagickaWeaponDamageKeyword not found"
      false
    else
      Log "MagickaWeaponSpellToCast and MagickaWeaponDamageKeyword found"
      true

  let onWeaponHit (eArg: OnHitWeaponArgs) =
    
    if eArg.Attacked <> null && eArg.Attacked.IsValid && eArg.Attacker <> null && eArg.Attacker.IsValid 
      && eArg.HitData.Weapon <> null && eArg.HitData.Weapon.IsValid
      && eArg.HitData.Weapon.HasKeyword(RefConfig.MagickaWeaponDamageKeyword.Value) then
      
      let spell = RefConfig.MagickaWeaponSpellToCast.Value
      
      let percentBlocked =
        let percent = eArg.HitData.PercentBlocked / 100.f
        if percent > 1.0f then 1.0f elif percent < 0.0f then 0.0f else percent

      let damage = eArg.HitData.TotalDamage * percentBlocked
      spell.Effects[0].Magnitude <- damage

      eArg.Attacker.CastSpell(spell, eArg.Attacked, eArg.Attacker)
      |> fun b -> if not b then Log "Failed cast Magicka weapon spell"

      eArg.ResultDamage

    else

      0.f