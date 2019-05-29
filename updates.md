Stormhalter v0.10
===================
I want to say a huge thanks to Slyster for his help on the last 10 versions. He was instrumental
in getting the client to a playable state.

Added: Networking protocol version to allow for future modification without breaking replays.
Fixed: Entities that can fly are not affected by water.
Fixed: Entities that can swim may drown, but entities that can fly will not.
Fixed: Spawns were not randomizing properly.
Fixed: Item descriptions were not properly localized.
Added: Message for when spell buffs have worn off from a spell source.
Fixed: Improved CombatAI spell selection, priority, and cooldowns.
Added: A tailor has relocated to provide armors, scales, and jackets.
Added: Chainmail and Platemail will now block spell casts.
Fixed: Respawns were not triggering properly.
Fixed: Fixed inclusion/exclusion calculations for spawn areas.
Fixed: Various LOS/Visibility issues with darkness.
Fixed: Round lock after emote and action command.
Fixed: Items may be inspected from a distance <= 1.
Fixed: Creatures will now regen mana, stamina, and health.
Added: Creatures have learned how to better use blind/fear/stun spell.
Fixed: Incorrect weapon sound when swinging projectile weapons.

Stormhalter v0.9
===================
Fixed: Crash related to items stacking. Item stacking will always consume a round.
Fixed: All container panels will reset when rewinding replay.
Added: Exit button to replay frame, returning back to login window.
Fixed: Interactions with items are only valid if on the same location.
Fixed: Spell sequence will properly reset only after all fizzle checks are complete.
Fixed: Venom will now prioritize right -> left -> target when using "cast" command.
Fixed: Jumpkick will now account for encumbrance.
Fixed: CasterAI will finish their cast after target moves into melee range.

Stormhalter v0.8
===================
Added: Server-side and Client-side replay functionality

Stormhalter v0.7
===================
Fixed: Reworked poison cloud, it should properly blind and apply poison ticks.
Fixed: Gems have been marked as a treasure and will not be removed by Disintegrate.
Fixed: Status effects from equipment will refresh after server restart.
Fixed: Added a slight delay to death sounds following fatal blow.
Fixed: Trainers will appropriately tell you when you below your potential skill.
Fixed: Trainers will now use your achieved skill for training, not current.
Fixed: Moving items may not always consume a round, especially in same container.
Fixed: Moving items within your hands and world will always consume a round.
Added: Nock/Unnock commands for projectile weapons.
Fixed: Container slots will respond to double clicks outside of the item sprite.
Fixed: Right clicks with an active target will now cancel to target.
Fixed: Right clicks will cancel any paths with priority over cancelling targets.
Fixed: Adjusted maximum creatures on Kesmai -1 from 48 to 60.
Fixed: Followers will be dispelled when reaching a limit.
Fixed: Cannot summon followers past the intended limit after relogging.
Fixed: Resolved an issue with Summon Phantasm where spell intensity reset to 1, 5.
Fixed: Throwable items would clear the round, allowing instant throws.
Fixed: Fire will burn corpses to a cinder.
Fixed: Creature will move onto hexes with portal effects.
Fixed: Floors will generate properly under walls that are destructible.
Fixed: Followers will now also respond to "#, (command)" directives.
Fixed: Added a "release" directive to followers, similar to "begone"
Fixed: Secret doors will have the appropriate destruction terrain.
Fixed: Adjusted the intensity/range of Concussion spell.
Fixed: CasterAI will now melee the target when enough mana is not available.
Fixed: Additional sprites will be rendered under the player.
Added: Mana regeneration provided by robes.

Stormhalter v0.6
===================
Fixed: Portals will cause damage when dispelled.
Fixed: Destroyed walls no longer block LOS.
Added: Prevented exit while feared, stunned, or blind.
Fixed: Swinging or throwing a projectile weapon results in little damage.
Fixed: Items are positioned randomly when all slots are occupied.
Fixed: Input will pass through items and container slots as needed.
Fixed: Spell icon for Stun/Death protection status.
Fixed: Items could not be equipped into the last portrait slot.
Fixed: Sounds for summoned Phantom, Djinn, and Efreets.
Fixed: Limited total summons from Summon Phantasm to 3.
Fixed: Removed corpses from phantasms.
Fixed: Thieves no longer have access to Darkness (1).
Fixed: Skill levels were rounded to integers when critiquing.
Fixed: Corrected certain item labels.

Stormhalter v0.5
===================
Fixed: Phantoms are properly summoned for spell power 1-5.
Fixed: Summoned creatures will now respond to move commands (creature, move x x x)
Added: Wizard's Eye for Wizards
Added: Entities can have colored bodies. Could be used as a mechanic to indicate creature status (enraged, pacified, stunned, etc.).
Fixed: An issue where departing clients were not properly removed from the segment.
Fixed: Lightning will now properly stun targets.
Fixed: Trainers will prevent you from logging out on their locations.
Fixed: Recall rings can not be equipped where exiting is not possible.
Fixed: Thunderclap from recalls will play properly at the destination.
Fixed: [Sandbox] Added debug messages for skill gain and combat.

Stormhalter v0.4
===================
Fixed: Skills can be critiqued from any distance.
Added: Swap command to switch item from occupied hand to a free hand.
Added: Wield command to switch item from the belt to a free hand.
Added: Belt command to switch items from your hand to a free slot.
Fixed: Reduced respawn time and increased random diversity on Kesmai -1
Fixed: Reduced respawn time and increased random diversity on Surface
Fixed: Caster AI melee attacks and combat messages
Fixed: A crash related to area of effect spells.
Fixed: Jumpkicks will no longer land after suffering a sprain.
Fixed: Summon rat sound and body perception
Fixed: Caster and Ranged AI will maintain distance from an attacker.
Added: [Sandbox] Heal command: Raises health, stamina, and mana to max.
Added: Game version will now display on the login screen.

Stormhalter v0.3
===================
Fixed: Changed compression library for smaller patch generation.
Fixed: A crash related to Summon Rat.
Fixed: Summon Rat spell did not properly transfer control to the caster.
Fixed: Magic skill gain.
Fixed: Poison cloud and Whirldwind will not pass through closed doors.
Fixed: Enchant and Venom will persist between server restarts.
Fixed: Ice/Fire breath have audio cues. Reduced sounds played for similar effects.
Fixed: Fixed a crash related to damage ticks after Fire/Ice are dispelled.
Fixed: Shield spell now has the same audio cue as other protection spells.
Fixed: Icestorm Spell did not place when cast with power level 1.
Fixed: Protection from Fire/Ice no longer causes an infinite round.
Fixed: Chant sounds for NPC entities
Fixed: Dropping item from one container to another, where an item exists, will drop the item to the next free slot. 

Stormhalter v0.2
===================
Fixed: It's now possible to look at the ground between actions.
Fixed: Trainers will now recognize attempts at training the hand skill.
Fixed: Between actions, item lift is now considered valid to prepare for a drop.
Fixed: Skeletons no longer have corpses.
Fixed: Wyverns on Kesmai -1 will no longer flee.
Fixed: Increased the sound delay between a swing and damage cue.
Fixed: Lars and Sven will recognize their service counters.
Fixed: Fizzle sounds will now play properly for spell sequence.
Fixed: Resolved incorrect sounds playing.
Fixed: Spellbooks are now properly bound when starting. 
Added: Client version verification to notify user of client updates.
Added: [Sandbox] Implement command - setskill {skillName} {0-19}
Added: [Sandbox] Go, Where command - go {x} {y} {regionId}
Fixed: Round will unlock after attempting to attack an invalid target.
Added: Tiles naturally covered in darkness until dispelled.
Fixed: Ruins will now interrupt movement.
Fixed: Trainers will now properly critique your progress.
Fixed: Mana will be properly consumed when failing spells.
Added: Changed encryption library parameters.

Stormhalter v0.1
===================