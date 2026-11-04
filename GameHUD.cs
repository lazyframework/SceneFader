using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private Image hpFill;
    [SerializeField] private Image mpFill;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text mpText;
    [SerializeField] private string shopSceneName = "ShopScene";
    [SerializeField] private string menuSceneName = "MainMenu";

    public void SetHP(float current, float max)
    {
        hpFill.fillAmount = Mathf.Clamp01(current / max);
        hpText.text = $"HP   {current:0} / {max:0}";
    }

    public void SetMP(float current, float max)
    {
        mpFill.fillAmount = Mathf.Clamp01(current / max);
        mpText.text = $"MP   {current:0} / {max:0}";
    }

    public void OpenShop() => SceneFader.Instance.LoadScene(shopSceneName);
    public void OpenMenu() => SceneFader.Instance.LoadScene(menuSceneName);

    public void OnReward() => Debug.Log("Reward ditekan");
    public void OnBag()    => Debug.Log("Bag ditekan");
    public void OnRun()    => Debug.Log("Lari ditekan");
    public void OnSkill1() => Debug.Log("Skill 1 ditekan");
    public void OnSkill2() => Debug.Log("Skill 2 ditekan");
    public void OnAttack() => Debug.Log("Serang ditekan");
    public void OnJump()   => Debug.Log("Lompat ditekan");
}
