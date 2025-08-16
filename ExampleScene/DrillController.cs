using Shababeek.Interactions;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(Grabable))]
public class DrillController : MonoBehaviour
{
    [SerializeField] private Transform drillBit;
    [SerializeField] private MeshRenderer[] meshRenderers;
    [SerializeField] private int materialIndex;
    [SerializeField] private string colorPropertyName = "_emissionColor";
    [SerializeField] private Color activationColor = Color.red;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private AudioSource motorSource;
    [SerializeField] private float drillSPeed;
    private Color _originalColor;
    private bool _isActive;

    void Start()
    {
        _originalColor = meshRenderers[0].materials[materialIndex].GetColor(colorPropertyName);
        var grabable = GetComponent<Grabable>();
        grabable.OnSelected
            .Do(_ => meshRenderers[0].materials[materialIndex].SetColor(colorPropertyName, selectedColor))
            .Subscribe()
            .AddTo(this);
        grabable.OnDeselected
            .Do(_ => meshRenderers[0].materials[materialIndex].SetColor(colorPropertyName, _originalColor))
            .Subscribe()
            .AddTo(this);
        grabable.OnUseStarted.Do(HandleUsing).Subscribe().AddTo(this);
        grabable.OnUseEnded.Do(HandleUnUsing).Subscribe().AddTo(this);
    }

    private void HandleUnUsing(InteractorBase obj)
    {
        throw new System.NotImplementedException();
    }

    private void HandleUsing(InteractorBase interactor)
    {
        motorSource?.Play();
        meshRenderers[0].materials[materialIndex].SetColor(colorPropertyName, activationColor);
        _isActive = true;
    }

    void Update()
    {
        if (_isActive)
        {
            drillBit.Rotate(0, drillSPeed * Time.deltaTime, 0, Space.Self);
        }
    }
}