using System;
using CWFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>선택지 시스템 이전의 기본 이벤트 화면. 설명을 읽고 확인하면 콘텐츠를 완료합니다.</summary>
[DisallowMultipleComponent]
public sealed class ExpeditionEventPanel : CBehaviour
{
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button continueButton;
    private Action completed;
    private bool bound;

    protected override void OnAwake()
    {
        if (completed == null) gameObject.SetActive(false);
    }

    public void Show(EventContent content, Action onCompleted)
    {
        if (content == null || onCompleted == null || description == null || continueButton == null)
            throw new InvalidOperationException("이벤트 콘텐츠, 설명 Text, 확인 Button을 연결해야 합니다.");
        if (!bound)
        {
            continueButton.onClick.AddListener(Continue);
            bound = true;
        }
        completed = onCompleted;
        description.text = content.description ?? string.Empty;
        continueButton.interactable = true;
        gameObject.SetActive(true);
    }

    private void Continue()
    {
        if (!isActiveAndEnabled || completed == null) return;
        var callback = completed;
        Hide();
        callback();
    }

    public void Hide()
    {
        completed = null;
        if (continueButton != null) continueButton.interactable = false;
        gameObject.SetActive(false);
    }

    protected override void OnReleased()
    {
        completed = null;
        if (bound && continueButton != null) continueButton.onClick.RemoveListener(Continue);
    }
}
