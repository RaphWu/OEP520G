using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Text;
using System.Windows.Controls;

namespace OEP520G.Automatic
{
    public class AutoActionSelector
    {
        // Singleton單例模式
        private static readonly Lazy<AutoActionSelector> lazy = new Lazy<AutoActionSelector>(() => new AutoActionSelector());
        public static AutoActionSelector Instance => lazy.Value;

        public List<AutoHead> Selector { get; set; }

        private AutoActionSelector()
        {
            BuildItems();
        }

        protected void BuildItems()
        {
            // Target
            static AutoTarget CreateTarget(ETarget id)
            {
                var ret = new AutoTarget()
                {
                    Id = id,
                    Key = id.ToString(),
                    Title = id.ToString()
                };

                return ret;
            }

            var AutoTarget_Stage = CreateTarget(ETarget.Stage);
            var AutoTarget_Tray01 = CreateTarget(ETarget.Tray01);
            var AutoTarget_Tray02 = CreateTarget(ETarget.Tray02);
            var AutoTarget_Tray03 = CreateTarget(ETarget.Tray03);
            var AutoTarget_Tray04 = CreateTarget(ETarget.Tray04);
            var AutoTarget_Tray05 = CreateTarget(ETarget.Tray05);
            var AutoTarget_Tray06 = CreateTarget(ETarget.Tray06);
            var AutoTarget_Tray07 = CreateTarget(ETarget.Tray07);
            var AutoTarget_Tray08 = CreateTarget(ETarget.Tray08);
            var AutoTarget_Tray09 = CreateTarget(ETarget.Tray09);
            var AutoTarget_Tray10 = CreateTarget(ETarget.Tray10);
            var AutoTarget_Tray11 = CreateTarget(ETarget.Tray11);
            var AutoTarget_Tray12 = CreateTarget(ETarget.Tray12);
            var AutoTarget_Tray13 = CreateTarget(ETarget.Tray13);
            var AutoTarget_Tray14 = CreateTarget(ETarget.Tray14);
            var AutoTarget_Tray15 = CreateTarget(ETarget.Tray15);
            var AutoTarget_Tray16 = CreateTarget(ETarget.Tray16);
            var AutoTarget_Tray17 = CreateTarget(ETarget.Tray17);
            var AutoTarget_Tray18 = CreateTarget(ETarget.Tray18);
            var AutoTarget_Tray19 = CreateTarget(ETarget.Tray19);
            var AutoTarget_Tray20 = CreateTarget(ETarget.Tray20);
            var AutoTarget_Tray21 = CreateTarget(ETarget.Tray21);
            var AutoTarget_Tray22 = CreateTarget(ETarget.Tray22);
            var AutoTarget_Tray23 = CreateTarget(ETarget.Tray23);
            var AutoTarget_Tray24 = CreateTarget(ETarget.Tray24);
            var AutoTarget_Tray25 = CreateTarget(ETarget.Tray25);
            var AutoTarget_Tray26 = CreateTarget(ETarget.Tray26);
            var AutoTarget_Tray27 = CreateTarget(ETarget.Tray27);
            var AutoTarget_Tray28 = CreateTarget(ETarget.Tray28);
            var AutoTarget_Tray29 = CreateTarget(ETarget.Tray29);
            var AutoTarget_Tray30 = CreateTarget(ETarget.Tray30);

            // Action
            AutoAction CreateAction_PickUp(EAction id)
            {
                var ret = new AutoAction()
                {
                    Id = id,
                    Key = id.ToString(),
                    Title = id.ToString()
                };
                ret.Targets.Add(AutoTarget_Tray01);
                ret.Targets.Add(AutoTarget_Tray02);
                ret.Targets.Add(AutoTarget_Tray03);
                ret.Targets.Add(AutoTarget_Tray04);
                ret.Targets.Add(AutoTarget_Tray05);
                ret.Targets.Add(AutoTarget_Tray06);
                ret.Targets.Add(AutoTarget_Tray07);
                ret.Targets.Add(AutoTarget_Tray08);
                ret.Targets.Add(AutoTarget_Tray09);
                ret.Targets.Add(AutoTarget_Tray10);
                ret.Targets.Add(AutoTarget_Tray11);
                ret.Targets.Add(AutoTarget_Tray12);
                ret.Targets.Add(AutoTarget_Tray13);
                ret.Targets.Add(AutoTarget_Tray14);
                ret.Targets.Add(AutoTarget_Tray15);
                ret.Targets.Add(AutoTarget_Tray16);
                ret.Targets.Add(AutoTarget_Tray17);
                ret.Targets.Add(AutoTarget_Tray18);
                ret.Targets.Add(AutoTarget_Tray19);
                ret.Targets.Add(AutoTarget_Tray20);
                ret.Targets.Add(AutoTarget_Tray21);
                ret.Targets.Add(AutoTarget_Tray22);
                ret.Targets.Add(AutoTarget_Tray23);
                ret.Targets.Add(AutoTarget_Tray24);
                ret.Targets.Add(AutoTarget_Tray25);
                ret.Targets.Add(AutoTarget_Tray26);
                ret.Targets.Add(AutoTarget_Tray27);
                ret.Targets.Add(AutoTarget_Tray28);
                ret.Targets.Add(AutoTarget_Tray29);
                ret.Targets.Add(AutoTarget_Tray30);

                return ret;
            }

            var AutoAction_PlacePart = new AutoAction()
            {
                Id = EAction.PlacePart,
                Key = EAction.PlacePart.ToString(),
                Title = EAction.PlacePart.ToString()
            };
            AutoAction_PlacePart.Targets.Add(AutoTarget_Stage);

            var AutoAction_PickUp01 = CreateAction_PickUp(EAction.PickUp01);
            var AutoAction_PickUp02 = CreateAction_PickUp(EAction.PickUp02);
            var AutoAction_PickUp03 = CreateAction_PickUp(EAction.PickUp03);
            var AutoAction_PickUp04 = CreateAction_PickUp(EAction.PickUp04);
            var AutoAction_PickUp05 = CreateAction_PickUp(EAction.PickUp05);
            var AutoAction_PickUp06 = CreateAction_PickUp(EAction.PickUp06);
            var AutoAction_PickUp07 = CreateAction_PickUp(EAction.PickUp07);
            var AutoAction_PickUp08 = CreateAction_PickUp(EAction.PickUp08);
            var AutoAction_PickUp09 = CreateAction_PickUp(EAction.PickUp09);
            var AutoAction_PickUp10 = CreateAction_PickUp(EAction.PickUp10);
            var AutoAction_PickUp11 = CreateAction_PickUp(EAction.PickUp11);

            var AutoAction_ImageCheck = new AutoAction()
            {
                Id = EAction.ImageCheck,
                Key = EAction.ImageCheck.ToString(),
                Title = EAction.ImageCheck.ToString()
            };

            var AutoAction_Dispensing = new AutoAction()
            {
                Id = EAction.Dispensing,
                Key = EAction.Dispensing.ToString(),
                Title = EAction.Dispensing.ToString()
            };
            AutoAction_Dispensing.Targets.Add(AutoTarget_Stage);

            var AutoAction_GlueAmountCheck = new AutoAction()
            {
                Id = EAction.GlueAmountCheck,
                Key = EAction.GlueAmountCheck.ToString(),
                Title = EAction.GlueAmountCheck.ToString()
            };
            AutoAction_Dispensing.Targets.Add(AutoTarget_Stage);

            var AutoAction_PreDispensing = new AutoAction()
            {
                Id = EAction.PreDispensing,
                Key = EAction.PreDispensing.ToString(),
                Title = EAction.PreDispensing.ToString()
            };
            AutoAction_Dispensing.Targets.Add(AutoTarget_Stage);

            var AutoAction_ClearGlue = new AutoAction()
            {
                Id = EAction.ClearGlue,
                Key = EAction.ClearGlue.ToString(),
                Title = EAction.ClearGlue.ToString()
            };
            AutoAction_Dispensing.Targets.Add(AutoTarget_Stage);

            // Head
            AutoHead CreateHead_Nozzle(EHead id)
            {
                var noz = new AutoHead()
                {
                    Id = id,
                    Key = id.ToString(),
                    Title = id.ToString()
                };
                noz.Actions.Add(AutoAction_PickUp01);
                noz.Actions.Add(AutoAction_PickUp02);
                noz.Actions.Add(AutoAction_PickUp03);
                noz.Actions.Add(AutoAction_PickUp04);
                noz.Actions.Add(AutoAction_PickUp05);
                noz.Actions.Add(AutoAction_PickUp06);
                noz.Actions.Add(AutoAction_PickUp07);
                noz.Actions.Add(AutoAction_PickUp08);
                noz.Actions.Add(AutoAction_PickUp09);
                noz.Actions.Add(AutoAction_PickUp10);
                noz.Actions.Add(AutoAction_PickUp11);

                return noz;
            }

            var AutoHead_Dispensor = new AutoHead()
            {
                Id = EHead.Dispensor,
                Key = EHead.Dispensor.ToString(),
                Title = EHead.Dispensor.ToString()
            };
            AutoHead_Dispensor.Actions.Add(AutoAction_Dispensing);
            AutoHead_Dispensor.Actions.Add(AutoAction_GlueAmountCheck);
            AutoHead_Dispensor.Actions.Add(AutoAction_PreDispensing);
            AutoHead_Dispensor.Actions.Add(AutoAction_ClearGlue);

            var AutoHead_Nozzle01 = CreateHead_Nozzle(EHead.Nozzle01);
            var AutoHead_Nozzle02 = CreateHead_Nozzle(EHead.Nozzle02);
            var AutoHead_Nozzle03 = CreateHead_Nozzle(EHead.Nozzle03);
            var AutoHead_Nozzle04 = CreateHead_Nozzle(EHead.Nozzle04);
            var AutoHead_Nozzle05 = CreateHead_Nozzle(EHead.Nozzle05);
            var AutoHead_Nozzle06 = CreateHead_Nozzle(EHead.Nozzle06);
            var AutoHead_Nozzle07 = CreateHead_Nozzle(EHead.Nozzle07);
            var AutoHead_Nozzle08 = CreateHead_Nozzle(EHead.Nozzle08);
            var AutoHead_Nozzle09 = CreateHead_Nozzle(EHead.Nozzle09);
            var AutoHead_Nozzle10 = CreateHead_Nozzle(EHead.Nozzle10);
            var AutoHead_Nozzle11 = CreateHead_Nozzle(EHead.Nozzle11);

            var AutoHead_Camrea = new AutoHead()
            {
                Id = EHead.Camera,
                Key = EHead.Camera.ToString(),
                Title = EHead.Camera.ToString()
            };
            AutoHead_Dispensor.Actions.Add(AutoAction_ImageCheck);

            //
            Selector = new List<AutoHead>
            {
                AutoHead_Dispensor,
                AutoHead_Nozzle01,
                AutoHead_Nozzle02,
                AutoHead_Nozzle03,
                AutoHead_Nozzle04,
                AutoHead_Nozzle05,
                AutoHead_Nozzle06,
                AutoHead_Nozzle07,
                AutoHead_Nozzle08,
                AutoHead_Nozzle09,
                AutoHead_Nozzle10,
                AutoHead_Nozzle11,
                AutoHead_Camrea
            };
        }
    }
}
