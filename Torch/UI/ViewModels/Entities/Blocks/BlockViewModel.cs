﻿using System;
using System.Collections.Generic;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Torch.Collections;

namespace Torch.UI.ViewModels.Entities.Blocks
{
    public class BlockViewModel : EntityViewModel
    {
        public BlockViewModel(IMyTerminalBlock block, EntityTreeViewModel tree) : base(block, tree)
        {
            if (Block == null)
                return;

            var propList = new List<ITerminalProperty>();
            block.GetProperties(propList);
            foreach (var prop in propList)
            {
                Type propType = null;
                foreach (var iface in prop.GetType().GetInterfaces())
                {
                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ITerminalProperty<>))
                        propType = iface.GenericTypeArguments[0];
                }

                var modelType = typeof(PropertyViewModel<>).MakeGenericType(propType);
                Properties.Add((PropertyViewModel)Activator.CreateInstance(modelType, prop, this));
            }
        }

        public BlockViewModel() { }

        public IMyTerminalBlock Block => (IMyTerminalBlock)Entity;
        public MtObservableList<PropertyViewModel> Properties { get; } = new MtObservableList<PropertyViewModel>();

        public string FullName => $"{Block?.CubeGrid.CustomName} - {Block?.CustomName}";

        public override string Name
        {
            get => Block?.CustomName ?? "null";
            set
            {
                TorchBase.Instance.Invoke(() =>
                {
                    Block.CustomName = value;
                    OnPropertyChanged();
                });
            }
        }

        /// <inheritdoc />
        public override string Position { get => base.Position; set { } }

        public long BuiltBy
        {
            get => ((MySlimBlock)Block?.SlimBlock)?.BuiltBy ?? 0;
            set
            {
                TorchBase.Instance.Invoke(() =>
                {
                    ((MySlimBlock)Block.SlimBlock).TransferAuthorship(value);
                    OnPropertyChanged();
                });
            }
        }

        public override bool CanStop => false;

        /// <inheritdoc />
        public override void Delete()
        {
            Block.CubeGrid.RazeBlock(Block.Position);
        }
    }
}