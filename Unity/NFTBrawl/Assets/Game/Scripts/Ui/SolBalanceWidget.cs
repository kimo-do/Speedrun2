using System;
using System.Collections;
using codebase.utility;
using Solana.Unity.SDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Ui
{
    /// <summary>
    /// Shows the sol balance of the connected wallet. Should be updated at certain points, after transactions for example.
    /// </summary>
    public class SolBalanceWidget : MonoBehaviour
    {
        public static SolBalanceWidget instance;
        public TextMeshProUGUI SolBalance;
        public TextMeshProUGUI SolChangeText;
        public TextMeshProUGUI PublicKey;
        public Button CopyAddressButton;

        private double lamportsChange;
        private Coroutine disableSolChangeCoroutine;
        private double currentLamports;
        private AudioSource audioSource;
        private bool hasPendingBalanceChange;
        private double pendingChange;
        private double pendingTotal;

        private void Awake()
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();

            if (CopyAddressButton)
            {
                CopyAddressButton.onClick.AddListener(OnCopyClicked);
            }
        }

        private void OnCopyClicked()
        {
            Clipboard.Copy(Web3.Account.PublicKey);
        }

        private void OnEnable()
        {
            Web3.OnBalanceChange += OnSolBalanceChangedMessage;
        }

        private void OnDisable()
        {
            Web3.OnBalanceChange -= OnSolBalanceChangedMessage;
        }
        
        private void UpdateContent()
        {
            SolBalance.text = currentLamports.ToString("F2") + " sol";
            if (PublicKey != null)
            {
                PublicKey.text = Web3.Account.PublicKey;
            }
        }

        private void OnSolBalanceChangedMessage(double newLamports)
        {
            double balanceChange = newLamports - currentLamports;

            if (balanceChange != 0 && Math.Abs(currentLamports - newLamports) > 0.00000001)
            {
                lamportsChange += balanceChange;
                if (balanceChange > 0)
                {
                    if (GameScreen.instance != null)
                    {
                        if (GameScreen.instance.HoldWalletUpdates)
                        {
                            hasPendingBalanceChange = true;
                            pendingChange = lamportsChange;
                            pendingTotal = newLamports;
                            return;
                        }
                    }

                    if (disableSolChangeCoroutine != null)
                    {
                        StopCoroutine(disableSolChangeCoroutine);
                    }

                    ShowGain();
                }
                else
                {
                    if (balanceChange < -0.0001)
                    {
                        if (disableSolChangeCoroutine != null)
                        {
                            StopCoroutine(disableSolChangeCoroutine);
                        }

                        SolChangeText.text = "<color=red>" + lamportsChange.ToString("F2") + "</color> ";
                        disableSolChangeCoroutine = StartCoroutine(DisableSolChangeDelayed());
                    }
                }

                currentLamports = newLamports;
                UpdateContent();
            }
            else
            {
                currentLamports = newLamports;
                UpdateContent();
            }
        }

        private void ShowGain()
        {
            SolChangeText.text = "<color=green>+" + lamportsChange.ToString("F2") + "</color> ";
            disableSolChangeCoroutine = StartCoroutine(DisableSolChangeDelayed());

            if (audioSource != null)
            {
                audioSource.Play();
            }
        }

        public void ShowPendingChanges()
        {
            if (hasPendingBalanceChange)
            {
                hasPendingBalanceChange = false;
                lamportsChange = pendingChange;
                currentLamports = pendingTotal;
                ShowGain();
                UpdateContent();
            }
        }

        private IEnumerator DisableSolChangeDelayed()
        {
            SolChangeText.gameObject.SetActive(true);
            yield return new WaitForSeconds(3);
            lamportsChange = 0;
            SolChangeText.gameObject.SetActive(false);
            disableSolChangeCoroutine = null;
        }
    }
}