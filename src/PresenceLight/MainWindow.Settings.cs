﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

using PresenceLight.Core.Services;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        private async Task LoadSettings()
        {
            if (!(await SettingsService.IsFilePresent()))
            {
                await SettingsService.SaveSettings(_options);
            }

            Config = await SettingsService.LoadSettings();

            if (string.IsNullOrEmpty(Config.RedirectUri))
            {
                await SettingsService.DeleteSettings();
                await SettingsService.SaveSettings(_options);
            }
            if (Config.LightSettings.UseWorkingHours)
            {
                pnlWorkingHours.Visibility = Visibility.Visible;
                SyncOptions();
            }
            else
            {
                pnlWorkingHours.Visibility = Visibility.Collapsed;
                SyncOptions();
            }

            if (Config.LightSettings.Hue.IsPhillipsHueEnabled)
            {
                pnlPhillips.Visibility = Visibility.Visible;
                pnlHueApi.Visibility = Visibility.Visible;
                SyncOptions();
            }
            else
            {
                pnlPhillips.Visibility = Visibility.Collapsed;
                pnlHueApi.Visibility = Visibility.Collapsed;
            }

            if (Config.LightSettings.Yeelight.IsYeelightEnabled)
            {
                pnlYeelight.Visibility = Visibility.Visible;
                SyncOptions();
            }
            else
            {
                pnlYeelight.Visibility = Visibility.Collapsed;
            }

            if (Config.LightSettings.LIFX.IsLIFXEnabled)
            {
                getTokenLink.Visibility = Visibility.Visible;
                pnlLIFX.Visibility = Visibility.Visible;

                SyncOptions();
            }
            else
            {
                getTokenLink.Visibility = Visibility.Collapsed;
                pnlLIFX.Visibility = Visibility.Collapsed;
            }

            if (Config.LightSettings.Custom.IsCustomApiEnabled)
            {
                pnlCustomApi.Visibility = Visibility.Visible;
                customApiAvailableMethod.SelectedValue = Config.LightSettings.Custom.CustomApiAvailableMethod;
                customApiBusyMethod.SelectedValue = Config.LightSettings.Custom.CustomApiBusyMethod;
                customApiBeRightBackMethod.SelectedValue = Config.LightSettings.Custom.CustomApiBeRightBackMethod;
                customApiAwayMethod.SelectedValue = Config.LightSettings.Custom.CustomApiAwayMethod;
                customApiDoNotDisturbMethod.SelectedValue = Config.LightSettings.Custom.CustomApiDoNotDisturbMethod;
                customApiAvailableIdleMethod.SelectedValue = Config.LightSettings.Custom.CustomApiAvailableIdleMethod;
                customApiOfflineMethod.SelectedValue = Config.LightSettings.Custom.CustomApiOfflineMethod;
                customApiOffMethod.SelectedValue = Config.LightSettings.Custom.CustomApiOffMethod;

                customApiActivityAvailableMethod.SelectedValue = Config.LightSettings.Custom.CustomApiActivityAvailableMethod;
                customApiActivityPresentingMethod.SelectedValue = Config.LightSettings.Custom.CustomApiActivityPresentingMethod;
                customApiActivityInACallMethod.SelectedValue = Config.LightSettings.Custom.CustomApiActivityInACallMethod;
                customApiActivityInAMeetingMethod.SelectedValue = Config.LightSettings.Custom.CustomApiActivityInAMeetingMethod;
                customApiActivityBusyMethod.SelectedValue = Config.LightSettings.Custom.CustomApiActivityBusyMethod;
                customApiActivityAwayMethod.SelectedValue = Config.LightSettings.Custom.CustomApiActivityAwayMethod;
                customApiActivityBeRightBackMethod.SelectedValue = Config.LightSettings.Custom.CustomApiActivityBeRightBackMethod;
                customApiActivityOfflineMethod.SelectedValue = Config.LightSettings.Custom.CustomApiActivityOfflineMethod;
                customApiActivityDoNotDisturbMethod.SelectedValue = Config.LightSettings.Custom.CustomApiActivityDoNotDisturbMethod;
                customApiActivityIdleMethod.SelectedValue = Config.LightSettings.Custom.CustomApiActivityIdleMethod;
                customApiActivityOffMethod.SelectedValue = Config.LightSettings.Custom.CustomApiActivityOffMethod;
                SyncOptions();
            }
            else
            {
                pnlCustomApi.Visibility = Visibility.Collapsed;
            }
            if (!string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXClientId) && !(string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXClientSecret)))
            {
                getTokenLink.Visibility = Visibility.Visible;
            }
            else
            {
                getTokenLink.Visibility = Visibility.Collapsed;
            }
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            btnSettings.IsEnabled = false;
            if (Transparent.IsChecked == true)
            {
                Config.IconType = "Transparent";
            }
            else
            {
                Config.IconType = "White";
            }

            if (HourStatusKeep.IsChecked == true)
            {
                Config.LightSettings.HoursPassedStatus = "Keep";
            }

            if (HourStatusOff.IsChecked == true)
            {
                Config.LightSettings.HoursPassedStatus = "Off";
            }

            if (HourStatusWhite.IsChecked == true)
            {
                Config.LightSettings.HoursPassedStatus = "White";
            }

            CheckAAD();
            Config.LightSettings.DefaultBrightness = Convert.ToInt32(brightness.Value);

            SetWorkingDays();

            SyncOptions();
            await SettingsService.SaveSettings(Config);
            lblSettingSaved.Visibility = Visibility.Visible;
            btnSettings.IsEnabled = true;
        }

        private void SetWorkingDays()
        {
            List<string> days = new List<string>();

            if (Monday.IsChecked.Value)
            {
                days.Add("Monday");
            }

            if (Tuesday.IsChecked.Value)
            {
                days.Add("Tuesday");
            }

            if (Wednesday.IsChecked.Value)
            {
                days.Add("Wednesday");
            }

            if (Thursday.IsChecked.Value)
            {
                days.Add("Thursday");
            }

            if (Friday.IsChecked.Value)
            {
                days.Add("Friday");
            }

            if (Saturday.IsChecked.Value)
            {
                days.Add("Saturday");
            }

            if (Sunday.IsChecked.Value)
            {
                days.Add("Sunday");
            }

            Config.LightSettings.WorkingDays = string.Join("|", days);
        }

        private void CheckAAD()
        {
            Regex r = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$");
            if (string.IsNullOrEmpty(Config.ClientId) || string.IsNullOrEmpty(Config.RedirectUri) || !r.IsMatch(Config.ClientId))
            {
                configErrorPanel.Visibility = Visibility.Visible;
                dataPanel.Visibility = Visibility.Hidden;
                signInPanel.Visibility = Visibility.Hidden;
                return;
            }

            SyncOptions();

            configErrorPanel.Visibility = Visibility.Hidden;

            if (dataPanel.Visibility != Visibility.Visible)
            {
                signInPanel.Visibility = Visibility.Visible;
            }

            if (_graphServiceClient == null)
            {
                _graphServiceClient = _graphservice.GetAuthenticatedGraphClient(typeof(WPFAuthorizationProvider));
            }
        }
        private void PopulateWorkingDays()
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingDays))
            {

                if (Config.LightSettings.WorkingDays.Contains("Monday"))
                {
                    Monday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Tuesday"))
                {
                    Tuesday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Wednesday"))
                {
                    Wednesday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Thursday"))
                {
                    Thursday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Friday"))
                {
                    Friday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Saturday"))
                {
                    Saturday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Sunday"))
                {
                    Sunday.IsChecked = true;
                }
            }
        }

        private async void cbSyncLights(object sender, RoutedEventArgs e)
        {
            if (!Config.LightSettings.SyncLights)
            {
                await SetColor("Off");
                turnOffButton.Visibility = Visibility.Collapsed;
                turnOnButton.Visibility = Visibility.Visible;
            }

            SyncOptions();
            await SettingsService.SaveSettings(Config);
            e.Handled = true;
        }

        private async void cbUseDefaultBrightnessChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.UseDefaultBrightness)
            {
                pnlDefaultBrightness.Visibility = Visibility.Visible;
            }
            else
            {
                pnlDefaultBrightness.Visibility = Visibility.Collapsed;
            }

            SyncOptions();
            await SettingsService.SaveSettings(Config);
            e.Handled = true;
        }

        private void cbUseWorkingHoursChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursStartTime))
            {
                Config.LightSettings.WorkingHoursStartTime = DateTime.Parse(Config.LightSettings.WorkingHoursStartTime).TimeOfDay.ToString();
            }

            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursEndTime))
            {
                Config.LightSettings.WorkingHoursEndTime = DateTime.Parse(Config.LightSettings.WorkingHoursEndTime).TimeOfDay.ToString();
            }

            if (Config.LightSettings.UseWorkingHours)
            {
                pnlWorkingHours.Visibility = Visibility.Visible;
            }
            else
            {
                pnlWorkingHours.Visibility = Visibility.Collapsed;
            }

            SyncOptions();
            e.Handled = true;
        }

        bool IsInWorkingHours()
        {
            if (string.IsNullOrEmpty(Config.LightSettings.WorkingHoursStartTime) || string.IsNullOrEmpty(Config.LightSettings.WorkingHoursEndTime) || string.IsNullOrEmpty(Config.LightSettings.WorkingDays))
            {
                IsWorkingHours = false;
                return false;
            }

            if (!Config.LightSettings.WorkingDays.Contains(DateTime.Now.DayOfWeek.ToString()))
            {
                IsWorkingHours = false;
                return false;
            }

            // convert datetime to a TimeSpan
            bool validStart = TimeSpan.TryParse(Config.LightSettings.WorkingHoursStartTime, out TimeSpan start);
            bool validEnd = TimeSpan.TryParse(Config.LightSettings.WorkingHoursEndTime, out TimeSpan end);
            if (!validEnd || !validStart)
            {
                IsWorkingHours = false;
                return false;
            }

            TimeSpan now = DateTime.Now.TimeOfDay;
            // see if start comes before end
            if (start < end)
            {
                IsWorkingHours = start <= now && now <= end;
                return IsWorkingHours;
            }
            // start is after end, so do the inverse comparison

            IsWorkingHours = !(end < now && now < start);

            return IsWorkingHours;
        }

        private void time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursStartTime))
            {
                Config.LightSettings.WorkingHoursStartTime = DateTime.Parse(Config.LightSettings.WorkingHoursStartTime).TimeOfDay.ToString();
            }

            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursEndTime))
            {
                Config.LightSettings.WorkingHoursEndTime = DateTime.Parse(Config.LightSettings.WorkingHoursEndTime).TimeOfDay.ToString();
            }

            SyncOptions();
            e.Handled = true;
        }
    }
}
