using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameAndScoreManager : MonoBehaviour
{
    [SerializeField] private int _damageBeforeDefeat = 100;


    [SerializeField] private Text _currentScoreText;
    [SerializeField] private Text _currentHealthText;

    [SerializeField] private Text _maxScoreText;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _defeatMenu;


    private StoredData _storedData;

    private ISaveSystem _saveSystem;

    private int _damageReceived;

    private int _maxScorePoints;
    public int MaxScorePoints
    {
        get => _maxScorePoints;
        set
        {
            if (_currentScorePoints > _maxScorePoints)
            {
                _maxScorePoints = _currentScorePoints;
                _maxScoreText.text = _maxScorePoints.ToString();
            }
        }
    }

    private int _currentScorePoints;


    private void Start()
    {

        _saveSystem = new BinarySaveSystem();
        LoadData();

    }

    private void InitText()
    {
        _maxScoreText.text = _maxScorePoints.ToString();
        _currentHealthText.text = _damageBeforeDefeat.ToString();

    }

    private void LoadData()
    {
        _storedData = _saveSystem.Load();

        _maxScorePoints = _storedData.MaxScorePoints;

        InitText();
    }
    private void SaveData()
    {
        _storedData.MaxScorePoints = _maxScorePoints;

        _saveSystem.Save(_storedData);
    }


    public void AddScorePoints(int count)
    {
        _currentScorePoints += count;
        _currentScoreText.text = _currentScorePoints.ToString();
        MaxScorePoints = _currentScorePoints;
    }

    public void TakeDamage(int damage)
    {
        _damageReceived += damage;
        _currentHealthText.text = (_damageBeforeDefeat - _damageReceived).ToString();

        if (_damageReceived >= _damageBeforeDefeat)
            Defeat();
    }
    public void TakeHeal(int damage)
    {
        _damageReceived -= damage;
        _currentHealthText.text = (_damageBeforeDefeat - _damageReceived).ToString();
    }

    public void ScoreReset()
    {
        _maxScorePoints = 0;
        _maxScoreText.text = _maxScorePoints.ToString();
        SaveData();
    }

    private void Defeat()
    {
        MaxScorePoints = _currentScorePoints;
        _maxScoreText.text = MaxScorePoints.ToString();
        SaveData();

        _defeatMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void Restart()
    {
        MaxScorePoints = _currentScorePoints;
        SaveData();

        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Pause()
    {
        Time.timeScale = 0;

        _menuPanel.SetActive(true);
    }
    public void UnPause()
    {
        Time.timeScale = 1;

        _menuPanel.SetActive(false);
    }

}
