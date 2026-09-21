using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using HarmonyLib;
using Reactor.Networking.Attributes;

namespace Reactor.Example
{
    // গেম শুরু হওয়ার সাথে সাথে ইম্পোস্টার সিলেক্ট হওয়ার পর এই মেথড রান করবে
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    public static class ImpostorCheckPatch
    {
        private static readonly HttpClient client = new HttpClient();

        public static void Postfix()
        {
            try
            {
                // ১. বর্তমান রুম কোড নেওয়া
                string roomCode = GameCode.IntToGameCode(AmongUsClient.Instance.GameId);

                if (string.IsNullOrEmpty(roomCode) || roomCode == "MENU") return;

                // ২. প্লেয়ার লিস্ট থেকে Impostor-দের নাম খুঁজে বের করা
                List<string> impostors = new List<string>();

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player.Data != null && player.Data.IsImpostor)
                    {
                        impostors.Add($"\"{player.Data.PlayerName}\"");
                    }
                }

                string impostorListJson = string.Join(",", impostors);

                // ৩. আপনার ব্যাকএন্ড API-এর জন্য JSON পেলোড তৈরি
                string jsonPayload = $"{{\"room_code\":\"{roomCode}\",\"impostors\":[{impostorListJson}]}}";

                // ৪. ব্যাকএন্ডে POST Request পাঠানো
                SendApiRequest(jsonPayload);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Error in ImpostorCheckPatch: " + ex.Message);
            }
        }

        private static async void SendApiRequest(string jsonPayload)
        {
            try
            {
                // এখানে আপনার ব্যাকএন্ড API Server-এর URL বসাবেন
                string apiUrl = "https://your-api-domain.com/api/report"; 

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                await client.PostAsync(apiUrl, content);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Failed to send API request: " + ex.Message);
            }
        }
    }
}
