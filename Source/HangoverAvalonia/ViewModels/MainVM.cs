using System;
using ApplicationServices;
using DataLayer;
using System.Collections.Generic;
using HangoverBase;
using ReactiveUI;

namespace HangoverAvalonia.ViewModels
{
	public partial class MainVM : ViewModelBase
    {
        public MainVM()
        {
			Load_databaseVM();
            Load_deletedVM();
        }
	}
}
