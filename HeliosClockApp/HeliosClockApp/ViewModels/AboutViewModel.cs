﻿using HeliosClockApp.Models;
using HeliosClockApp.Services;
using HeliosClockApp.Views;
using HeliosClockCommon.Enumerations;
using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HeliosClockApp.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "Set Color";
            SetRandomolorCommand = new Command<ColorModel>(async (colorModel) =>
            {
                var rand = new Random();
                HeliosService.StartColor = colorModel.StartColor;
                HeliosService.EndColor = colorModel.EndColor;
                await HeliosService.SendColor().ConfigureAwait(false);
                //await Browser.OpenAsync("https://aka.ms/xamarin-quickstart");
            });

            BlackCommand = new Command(async () =>
            {
                HeliosService.StartColor = System.Drawing.Color.Black;
                HeliosService.EndColor = System.Drawing.Color.Black;
                await HeliosService.SetOnOff("off").ConfigureAwait(false);
            });

            WhiteCommand = new Command(async () =>
            {
                HeliosService.StartColor = System.Drawing.Color.White;
                HeliosService.EndColor = System.Drawing.Color.White;
                await HeliosService.SetOnOff("on").ConfigureAwait(false);
            });

            StartSpinCommand = new Command(async () =>
            {
                await HeliosService.StartMode(LedMode.Spin).ConfigureAwait(false);
            });

            StartKnightRiderCommand = new Command(async () =>
            {
                await HeliosService.StartMode(LedMode.KnightRider).ConfigureAwait(false);
            });

            StopCommand = new Command(async () =>
            {
                await HeliosService.StopMode().ConfigureAwait(false);
            });

            SetRefreshRateCommand = new Command<double>(async (speed) =>
            {
                await HeliosService.SetRefreshSpeed((int)speed).ConfigureAwait(false);
            });

            AddGradientCommand = new Command(OnAddGradient);
        }

        public ICommand SetRandomolorCommand { get; }

        public ICommand BlackCommand { get; }

        public ICommand WhiteCommand { get; }

        public ICommand StartKnightRiderCommand { get; }

        public ICommand SetRefreshRateCommand { get; }

        public ICommand AddGradientCommand { get; }

        public ICommand StartSpinCommand { get; }

        public ICommand StopCommand { get; }

        private async void OnAddGradient(object obj)
        {
            await Shell.Current.GoToAsync(nameof(GradientColorPage));
        }
    }
}