using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;  // Necessário para trabalhar com UI Image e UI Text

public class UITimer : MonoBehaviour
{
    [SerializeField] private Image radialTimerImage;  // Referência para a imagem do timer (UI Image)
    [SerializeField] private TextMeshProUGUI timerText;  // Referência para o texto que exibe o tempo restante
    [SerializeField] private float timerDuration = 5f;  // A duração total do timer (5 segundos)
    private float timer;  // Contador do tempo

    private void Start()
    {
        // Inicializa o timer
        if (radialTimerImage != null)
        {
            radialTimerImage.fillAmount = 1f;  // Começa com o timer cheio
        }
        if (timerText != null)
        {
            timerText.text = timer.ToString("F0");  // Mostra o tempo restante no formato de 1 casa decimal
        }
        timer = timerDuration;  // Define o tempo total do timer
    }

    private void Update()
    {
        // Subtrai o tempo no timer a cada frame
        if (timer > 0)
        {
            timer -= Time.deltaTime;  // Diminui o tempo
            float fillAmount = Mathf.Clamp01(timer / timerDuration);  // Calcula o preenchimento proporcional
            radialTimerImage.fillAmount = fillAmount;  // Atualiza o preenchimento da imagem

            // Atualiza o texto com o tempo restante (com 1 casa decimal)
            if (timerText != null)
            {
                timerText.text = timer.ToString("F0");  // Mostra o tempo restante no formato de 1 casa decimal
            }
        }
        else
        {
            // Timer terminou, chame o evento ou lógica que você deseja aqui
            // ShowRevivePanel();
            OnTimerEnd?.Invoke();
        }
    }

    public UnityEvent OnTimerEnd;

    // Função que reinicia o timer, se necessário
    public void ResetTimer()
    {
        timer = timerDuration;  // Restaura o tempo total
        radialTimerImage.fillAmount = 1f;  // Restaura o preenchimento do radial timer

        if (timerText != null)
        {
            timerText.text = timer.ToString("F0");  // Mostra o tempo restante no formato de 1 casa decimal
        }
    }

    // Função para exibir o painel de reviver (ou qualquer lógica que você precise)
    private void ShowRevivePanel()
    {
        // Ative o painel ou faça qualquer outra ação necessária quando o timer terminar
        Debug.Log("Timer Finalizado!");
        // Aqui você pode ativar um painel de UI de reviver, por exemplo:
        // revivePanel.SetActive(true);
    }
}
