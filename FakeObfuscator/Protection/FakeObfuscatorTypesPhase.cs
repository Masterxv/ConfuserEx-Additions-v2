﻿using Confuser.Core;
using Confuser.Core.Helpers;
using Confuser.Core.Services;
using Confuser.Renamer;
using Confuser.Runtime.FakeObfuscator;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Confuser.Protections.FakeObuscator
{
    public class FakeObfuscatorTypesPhase : ProtectionPhase
    {
        public override ProtectionTargets Targets => ProtectionTargets.Modules;
        public override string Name => "Fake obfuscator type addition";

        public FakeObfuscatorTypesPhase(ConfuserComponent parent) : base(parent) { }

        protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {
            var marker = context.Registry.GetService<IMarkerService>();
            var name = context.Registry.GetService<INameService>();
            var allAddedTypes = new List<IDnlibDef>();

            Type[][] typesToAdd = {
                BabelDotNet.GetTypes(),     //+110
                CodeFort.GetTypes(),        //+100
                CodeWall.GetTypes(),        //+100
                CryptoObfuscator.GetTypes(),//+120
                Dotfuscator.GetTypes(),     //+100
                EazfuscatorDotNet.GetTypes(),//+100
                GoliathDotNet.GetTypes(),   //+100
                Xenocode.GetTypes()         //+100
            };

            foreach (var m in parameters.Targets.Cast<ModuleDef>().WithProgress(context.Logger))
            {
                //inject types
                foreach (Type[] idk in typesToAdd.WithProgress(context.Logger))
                    allAddedTypes.AddRange(InjectType(m, context.Logger, idk));

                //mark types
                foreach (IDnlibDef def in allAddedTypes)
                {
                    marker.Mark(def, Parent);
                    name.SetCanRename(def, false);
                } 
            }
        }

        private static IEnumerable<IDnlibDef> InjectType(ModuleDef m, Core.ILogger l, params Type[] types)
        {
            List<IDnlibDef> ret = new List<IDnlibDef>();

            foreach (TypeDef type in types.Select(Utils.GetRuntimeType))
            {
                var newType = new TypeDefUser(m.GlobalType.Namespace, type.Name);
                m.Types.Add(newType);

                l.Debug("Added type " + newType);

                ret.Add(newType);
                ret.AddRange(InjectHelper.Inject(type, newType, m));
            }

            return ret;
        }
    }
}
