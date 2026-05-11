using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    [Header("Referensi UI")]
    [SerializeField] private RectTransform _dialoguePanel;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private RectTransform _waluyoIcon;
    [SerializeField] private RectTransform _ustadzIcon;

    [Header("Pengaturan Animasi")]
    [SerializeField] private float _typewriterSpeed = 0.05f;
    [SerializeField] private float _wiggleAngle = 10f;
    [SerializeField] private float _bobAmount = 15f;

    [Header("Input System")]
    [SerializeField] private InputActionReference _interactAction;

    private Queue<DialogueLine> _sentences = new Queue<DialogueLine>();
    private bool _isTyping = false;
    private bool _cancelTyping = false;
    private string _currentSentence;
    private Vector3 _originalPanelScale;
    
    // Variabel baru untuk mengecek apakah ini dialog terakhir
    private bool _isEndGame = false;

    private void Awake()
    {
        Instance = this;
        _originalPanelScale = _dialoguePanel.localScale;
        _dialoguePanel.localScale = Vector3.zero;
        _dialoguePanel.gameObject.SetActive(false);
    }

    // Fungsi StartDialogue kita beri tambahan parameter (bool isEndGameDialogue)
    public void StartDialogue(List<DialogueLine> dialogueGroup, bool isEndGameDialogue = false)
    {
        _isEndGame = isEndGameDialogue;

        // Freeze Game & Player
        if (GameManager.Instance != null) GameManager.Instance.isGameActive = false;
        
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.enabled = false;

        // Munculkan Panel
        _dialoguePanel.gameObject.SetActive(true);
        _dialoguePanel.DOScale(_originalPanelScale, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        _sentences.Clear();
        foreach (DialogueLine line in dialogueGroup)
        {
            _sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (_sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = _sentences.Dequeue();
        _nameText.text = currentLine.name;
        _currentSentence = currentLine.sentence;

        HandleIconVisibility(currentLine.isWaluyo);
        
        StopAllCoroutines();
        StartCoroutine(TypeSentence(_currentSentence));
        
        AudioManager.instance.PlayDialog("Dialog");

    }

    private void HandleIconVisibility(bool isWaluyoTalk)
    {
        _waluyoIcon.DOKill();
        _ustadzIcon.DOKill();

        if (isWaluyoTalk)
        {
            _waluyoIcon.gameObject.SetActive(true);
            _ustadzIcon.gameObject.SetActive(false);

            _waluyoIcon.localRotation = Quaternion.identity;
            _waluyoIcon.DOLocalRotate(new Vector3(0, 0, _wiggleAngle), 0.2f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        }
        else
        {
            _ustadzIcon.gameObject.SetActive(true);
            _waluyoIcon.gameObject.SetActive(false);

            _ustadzIcon.DOLocalMoveY(_ustadzIcon.localPosition.y + _bobAmount, 0.3f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        _dialogueText.text = "";
        _isTyping = true;
        _cancelTyping = false;

        foreach (char letter in sentence.ToCharArray())
        {
            if (_cancelTyping)
            {
                _dialogueText.text = sentence;
                break;
            }

            _dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(_typewriterSpeed); // Pakai realtime agar aman saat di-pause
        }

        _isTyping = false;
        _cancelTyping = false;
        AudioManager.instance.StopDialog();
    }

    public void EndDialogue()
    {
        _dialoguePanel.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
            _dialoguePanel.gameObject.SetActive(false);
            
            // CEK APAKAH INI DIALOG ENDGAME (SAMA PAK USTADZ)
            if (_isEndGame)
            {
                AudioManager.instance.StopMusic();
                // Jika iya, panggil UI Win Panel
                if (LevelEndUI.Instance != null)
                {
                    LevelEndUI.Instance.ShowWinPanel();
                }
            }
            else
            {
                // JIKA BUKAN ENDGAME (DIALOG AWAL)
                // --- KEMBALIKAN SEMUA KONTROL PLAYER DI SINI ---
                
                Time.timeScale = 1f;

                AudioManager.instance.PlayMusic("Gameplay");

                if (GameManager.Instance != null) 
                {
                    GameManager.Instance.isGameActive = true;
                }
                
                // Kembalikan pergerakan karakter
                PlayerMovement player = FindObjectOfType<PlayerMovement>();
                if (player != null) player.enabled = true;

                // Kembalikan kamera dan sembunyikan kursor
                CameraController cam = FindObjectOfType<CameraController>();
                if (cam != null) cam.enabled = true;

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        });
    }

    private void Update()
    {
        if (_interactAction != null && _interactAction.action.WasPressedThisFrame())
        {
            // Pastikan panel dialog sedang aktif sebelum memproses input
            if (!_dialoguePanel.gameObject.activeSelf) return;

            if (_isTyping)
            {
                _cancelTyping = true;
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }
}