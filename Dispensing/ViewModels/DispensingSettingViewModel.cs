using OEP520G.Core.Helpers;
using OEP520G.Database.Constants;
using OEP520G.Database.Contracts;
using OEP520G.Database.Models;
using OEP520G.Dispensing.Contracts;
using OEP520G.Dispensing.Models;
using OEP520G.ProductManager.Contracts;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OEP520G.Dispensing.ViewModels
{
    public class DispensingSettingViewModel : BindableBase, IActiveAware
    {
        private static int _lastShape = -1;

        // View Active/Deactive & ApplicationCommands
        private bool _IsActive = false;
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                _IsActive = value;
                OnIsActiveChanged();

                if (value)
                {
                    if (_lastShape > 0)
                        ShapeChangeover(_lastShape);
                    //UpdateShapeList();
                }
            }
        }
        public event EventHandler IsActiveChanged;
        //public DelegateCommand ApplyDataCommand { get; private set; }
        private void OnIsActiveChanged()
        {
            //ApplyDataCommand.IsActive = IsActive;
            IsActiveChanged?.Invoke(this, new EventArgs());
        }

        private readonly ICrudDialog _crudDialog;
        private readonly IProductManager _pm;
        private readonly IDispensing _dispensing;

        // ctor
        public DispensingSettingViewModel(ICrudDialog crudDialog,
                                          IProductManager productManager,
                                          IDispensing dispensing)
        {
            _crudDialog = crudDialog;
            _pm = productManager;
            _dispensing = dispensing;

            UpdateShapeList();
            DispensingShape = 0;

            //ApplyDataCommand = new DelegateCommand(ExecuteApplyDataCommand);
            //applicationCommands.ApplyDataCommand.RegisterCommand(ApplyDataCommand);
        }

        /********************
         * CRUD
         ********************/
        // 新建品種
        private DelegateCommand _NewDispenserCommand;
        public DelegateCommand NewDispenserCommand
            => _NewDispenserCommand ??= new DelegateCommand(ExecuteNewDispenserCommand);
        void ExecuteNewDispenserCommand()
        {
            CrudInfo crudInfo = new CrudInfo()
            {
                Action = CrudAction.Create,
                Title = LocalizationProvider.GetValue<string>("CRUD_New_Title"),
                NewName = "",
                NewTitle = LocalizationProvider.GetValue<string>("CRUD_New_NewTitle")
            };
            CrudInfo result = _crudDialog.ShowDialog(crudInfo);

            if (result.Result == ButtonResult.OK)
            {
                if (_dispensing.IsShapeExist(result.NewName))
                {
                    // Error
                    string msg = LocalizationProvider.GetValue<string>("ErrorMsg_ShapeExist") + result.NewName;
                    MessageBox.Show(msg,
                                    LocalizationProvider.GetValue<string>("ErrorMsg_Database_Title"),
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }

                _dispensing.CreateNewShape(result);
                UpdateShapeList();
            }
        }

        /********************
         * Command Binding
         ********************/
        //
        private DelegateCommand<object> _ShapeChangedCommand;
        public DelegateCommand<object> ShapeChangedCommand
             => _ShapeChangedCommand ??= new DelegateCommand<object>(ExecuteShapeChangedCommand);
        void ExecuteShapeChangedCommand(object selectedItem)
        {
            if (selectedItem != null && selectedItem is DispensingShapeDefine shape)
            {
                ShapeChangeover(shape.Id);
                _lastShape = shape.Id;
            }
        }

        // 點膠測試
        private DelegateCommand _DispensingTestCommand;
        public DelegateCommand DispensingTestCommand
            => _DispensingTestCommand ??= new DelegateCommand(ExecuteDispensingTestCommand);
        void ExecuteDispensingTestCommand()
        {

        }

        /********************
         * Functions
         ********************/
        private void UpdateShapeList()
        {
            DispensingShapeSource = null;
            DispensingShapeSource = DispensingParameters.Shape;
        }

        private void ShapeChangeover(int shapeId)
        {
            DispensingActionSource = DispensingParameters.Sequence.FindAll(x => x.ShapeId == shapeId);
        }

        /********************
         * Data Binding
         ********************/
        private List<DispensingShapeDefine> _DispensingShapeSource;
        public List<DispensingShapeDefine> DispensingShapeSource
        {
            get { return _DispensingShapeSource; }
            set { SetProperty(ref _DispensingShapeSource, value); }
        }

        private int _DispensingShape;
        public int DispensingShape
        {
            get { return _DispensingShape; }
            set { SetProperty(ref _DispensingShape, value); }
        }

        private List<DispensingSequenceDefine> _DispensingActionSource;
        public List<DispensingSequenceDefine> DispensingActionSource
        {
            get { return _DispensingActionSource; }
            set { SetProperty(ref _DispensingActionSource, value); }
        }
    }
}
