using Rito.ObjectPooling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Utility.Theme;

public class Apple10TextBox : MonoBehaviour
{
    const int MAX_ROW = 17, MAX_COL = 10;
    //const int ROW = 5, COL = 5;
    const int MAX_NUM = 10; // 모든 숫자가 1일 때 최대 10칸까지 가능
    const int MAX_CELLS = 16; // 모든 숫자가 1일 때 최대 10칸까지 가능
    byte[,] sumTable = new byte[MAX_ROW + 1, MAX_COL + 1]; // 경계 처리를 위해 1씩 크게 설정
    byte[,] countTable = new byte[MAX_ROW + 1, MAX_COL + 1];
    byte[,] grid = new byte[MAX_ROW, MAX_COL];
    [SerializeField] Canvas canv;
    [SerializeField] RectTransform selectionBox;
    [SerializeField] TMP_Text mainText;
    [SerializeField] AudioClip removeCharSound;
    AudioSource audio;
    StringBuilder sb = new(400); // 미리 용량 확보 및 재사용
    List<int> selectedIndices = new(10);
    Camera cam;
    Vector3 startPos, endPos;
    Color mainColor, subColor, highlightColor, shuffleColor;
    float scalePer;
    public int seed { get; private set; } = 0;
    int selectedIndexSum = 0;
    int currentRow;
    int currentCol;
    bool isDragging = false;
    bool isPause = false;
    bool isCustom;
    bool onClickHint;
    public bool particleOff, hintEnable;
    void Awake()
    {
        if(!mainText)
            TryGetComponent(out mainText);
        cam = Camera.main;
        scalePer = 1 / canv.scaleFactor;
        if (!TryGetComponent(out audio))
            Debug.Log($"No AudioSource In {name}");
    }

    public void Initialized(byte[,] _arr = null)
    {
        isCustom = _arr != null;
        currentRow = isCustom ? _arr.GetLength(0) : MAX_ROW;
        currentCol = isCustom ? _arr.GetLength(1) : MAX_COL;

        GenerateGrid(_arr);
        
        seed = UnityEngine.Random.Range(1, int.MaxValue);
        UnityEngine.Random.InitState(seed);
        mainText.color = mainColor;
        selectionBox.gameObject.SetActive(false);
        ChageTheme();
        StartCoroutine(ShuffleRutine());
    }
    void Update()
    {
        if (isPause) return;
        
        HandleMouseInput();
        /*if (Input.GetKeyDown(KeyCode.A))
            ShowHint();
        if (Input.GetKeyDown(KeyCode.S))
            OnGameEnd();
        if (Input.GetKeyDown(KeyCode.D))
            StartCoroutine(ShuffleRutine());
        if (Input.GetKeyDown(KeyCode.X))
            PlayerPrefs.DeleteAll();
        if (Input.GetKeyDown(KeyCode.Z))
            Debug.Log($"{SaveManager.I.GetData(EnumData.IsTutorialEnd, 0)} /{PlayerPrefs.HasKey(nameof(EnumData.IsTutorialEnd))}");
        */
        onClickHint = false;
    }
    private void OnEnable()
    {
        ThemeManager.Instance.OnThemeChanged += ChageTheme;
    }
    private void OnDisable()
    {
        ThemeManager.Instance.OnThemeChanged -= ChageTheme;

    }
    void OnChangeNumber()
    {
        UpdatePrefixSum();
        if (IsClear)
        {
            GameManager.Instance.OnClear();
            return;
        }
        if (!HasPossibleMoves())
            StartCoroutine(ShuffleRutine());
    }
    public void ChageTheme()
    {
        ThemeData _theme = ThemeManager.Instance.currentTheme;
        //ThemeData theme = themeData;
        mainColor = _theme.textMain;
        subColor = _theme.textSub;
        highlightColor = _theme.accent;
        shuffleColor = _theme.number;
    }

    void GenerateGrid(byte[,] _arr = null)
    {// 1~9 사이의 랜덤 숫자로 격자 초기화
        for (int r = 0; r < currentRow; r++)
            for (int c = 0; c < currentCol; c++)
                grid[r, c] = isCustom ? _arr[r, c] : (byte)UnityEngine.Random.Range(1, MAX_NUM);
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isDragging = true;
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(true); 
        }

        if (Input.GetMouseButton(0))
        {
            if (!isDragging) return;
            endPos = Input.mousePosition;
            CheckNumbersInRange();
            UpdateHighlightRect();
        }

        if (Input.GetMouseButtonUp(0) )
        {
            isDragging = false;
            selectionBox.gameObject.SetActive(false);
            if (onClickHint)
                return;
            SetAllColor(mainColor);
            CheckSum();
            // 변경사항 적용
            mainText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
        
    }

    void CheckSum()
    {
        if (selectedIndexSum == MAX_NUM)
        {
            foreach (var index in selectedIndices)
                RemoveChar(index);
            GameManager.Instance.AddScore(selectedIndices.Count);
            selectedIndices.Clear();


            PlaySound();
            OnChangeNumber();
        }
    }

    void UpdateTextDisplay(bool _effect = false)
    {// 배열 데이터를 TMP 텍스트 형식으로 변환
        sb.Clear();
        for (int r = 0; r < currentRow; r++)
        {// 숫자가 0이면 지워진 것으로 간주하여 공백 처리
            for (int c = 0; c < currentCol; c++)
            {
                int num = !_effect ? grid[r, c] : UnityEngine.Random.Range(1, 10);
                if (num == 0) sb.Append("  ");
                else sb.Append($"{num} ");
            }
            sb.Append("\n");
        }
        mainText.color = isPause ? shuffleColor : mainColor;
        mainText.SetText(sb);
        // 텍스트 메쉬 정보를 즉시 갱신 (좌표 계산을 위해 필수)
        //mainText.ForceMeshUpdate();
    }

    void UpdateHighlightRect()
    {// 마우스 시작점과 현재점 기준으로 사각형 Rect를 계산 (스크린 좌표)
        if (selectionBox == null) return;
        float xMin = Mathf.Min(startPos.x, endPos.x);
        float yMin = Mathf.Min(startPos.y, endPos.y);
        float xMax = Mathf.Max(startPos.x, endPos.x);
        float yMax = Mathf.Max(startPos.y, endPos.y);

        float width = xMax - xMin;
        float height = yMax - yMin;

        // UI Image의 RectTransform을 조작하여 드래그 영역 표시
        RectTransform rt = selectionBox;

        // ScreenPointToLocalPointInRectangle을 사용하여 캔버스 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canv.transform as RectTransform, // 캔버스 자체의 RectTransform 사용
            new (xMin + width *.5f, yMin + height *.5f), // 사각형의 중앙점 스크린 좌표
            cam,
            out Vector2 localRectCenter
        );

        rt.localPosition = localRectCenter;
        rt.sizeDelta = new Vector2(width, height) * scalePer;
    }
    void CheckNumbersInRange()
    {// 드래그 영역 설정 (Screen Space)
        Rect dragRect = new (
            Mathf.Min(startPos.x, endPos.x),
            Mathf.Min(startPos.y, endPos.y),
            Mathf.Abs(startPos.x - endPos.x),
            Mathf.Abs(startPos.y - endPos.y)
        );

        selectedIndexSum = 0;
        TMP_TextInfo textInfo = mainText.textInfo;
        selectedIndices.Clear();

        for (int i = 0; i < textInfo.characterCount; i++)
        {// 모든 글자를 순회하며 드래그 범위 안에 있는지 확인
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // 글자가 유효하고(보이고), 줄바꿈이 아닌 경우만 체크
            if (!charInfo.isVisible) continue;

            // 글자의 월드 좌표를 스크린 좌표로 변환
            Vector3 charCenter = (charInfo.bottomLeft + charInfo.topRight) * 0.5f;
            Vector3 worldPos = mainText.transform.TransformPoint(charCenter);
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
            Color32 color = mainColor;

            if (dragRect.Contains(screenPos))
            {// 텍스트 인덱스를 2차원 배열 인덱스로 역산
                // (공백과 줄바꿈을 고려하여 원본 grid에서 값을 찾음)
                Vector2Int pos = IndexToPos(i);

                int meshIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;

                int num = grid[pos.x, pos.y];
                if (num == 0) continue;

                if (pos.x < currentRow && pos.y < currentCol && vertexColors[vertexIndex].a > 0)
                {
                    if (selectedIndices.Count < 10)
                        selectedIndices.Add(i);
                    selectedIndexSum += num;
                    color = subColor;
                }
            }
            SetCharColor(i, color);
        }

        mainText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
    void SetCharColor(int _index, Color32 _color)
    {// 특정 인덱스의 글자 색상만 바꾸는 저수준 함수
        TMP_TextInfo textInfo = mainText.textInfo;

        // 이미 지워진 글자(알파 0)는 색상 변경 대상에서 제외
        int meshIndex = textInfo.characterInfo[_index].materialReferenceIndex;
        int vertexIndex = textInfo.characterInfo[_index].vertexIndex;
        Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;

        if (vertexColors == null || vertexColors[vertexIndex].a == 0) return;

        // 4개의 정점 색상을 모두 변경
        for (int i = 0; i < 4; i++)
            vertexColors[vertexIndex + i] = _color;
    }
    void SetAllColor(Color32 _color)
    {
        for (int i = 0, len = mainText.textInfo.characterCount; i < len; i++)
            SetCharColor(i, _color);
    }
    void RemoveChar(int _index , bool sound = false)
    {
        SetCharColor(_index, Color.clear);

        TMP_TextInfo textInfo = mainText.textInfo;
        TMP_CharacterInfo charInfo = textInfo.characterInfo[_index];
        Vector3 localCenter = (charInfo.bottomLeft + charInfo.topRight) * 0.5f;
        Vector3 worldCenter = mainText.transform.TransformPoint(localCenter);

        var pos = IndexToPos(_index);
        //Debug.Log($"{grid[pos.x, pos.y]} -> 0");
        grid[pos.x, pos.y] = 0;

        if (sound)
            PlaySound();

        if (particleOff)
            return;

        var obj = ObjectPoolManager.I.Spawn("Fireworks_Small");
        obj.transform.position = worldCenter;
        obj.TryGetComponent(out ParticleSystem particle);
        var main = particle.main;
        main.startColor = mainColor;
        ObjectPoolManager.I.Despawn(obj, .5f);

    }

    void UpdatePrefixSum()
    {// 누적합 테이블 갱신 (숫자가 바뀔 때 호출)
        for (int r = 1; r <= currentRow; r++)
        {
            for (int c = 1; c <= currentCol; c++)
            {
                int val = grid[r - 1, c - 1];
                int isPopulated = (val != 0) ? 1 : 0;

                // 숫자 합 누적
                sumTable[r, c] = (byte)(val + sumTable[r - 1, c] + sumTable[r, c - 1] - sumTable[r - 1, c - 1]);

                // 숫자 개수 누적
                countTable[r, c] = (byte)(isPopulated + countTable[r - 1, c] + countTable[r, c - 1] - countTable[r - 1, c - 1]);
            }
        }
    }

    public bool HasPossibleMoves()
    {
        for (int r1 = 0; r1 < currentRow; r1++)
            for (int c1 = 0; c1 < currentCol; c1++)
            {
                //if (grid[r1, c1] == 0) continue;

                // r2는 r1으로부터 최대 10행까지만 (현실적으로는 더 적음)
                //int rowLimit = Mathf.Min(r1 + MAX_CELLS, ROW);
                for (int r2 = r1; r2 < currentRow; r2++)
                    for (int c2 = c1; c2 < currentCol; c2++)
                    {// 가로 x 세로 면적이 10을 초과하면 합이 10일 수가 없음 (최소 숫자가 1이므로)
                        int area = GetActiveCountFast(r1, c1, r2, c2);
                        if (area < 2) continue;
                        if (area > MAX_CELLS) break; // 현재 r2에서 c2를 더 키우는 건 의미 없음

                        int currentSum = GetRectSumFast(r1, c1, r2, c2);

                        if (currentSum == MAX_NUM) return true;
                        else if (currentSum > MAX_NUM) break; // 합이 넘어가도 중단
                    }
                
            }
        
        return false;
    }
    public void ShowHint()
    {// 텍스트 구조: (숫자 + 공백) * 10개 + 줄바꿈(\n)
        onClickHint = true;
        for (int r1 = 0; r1 < currentRow; r1++)
            for (int c1 = 0; c1 < currentCol; c1++)
            {
                //if (grid[r1, c1] == 0) continue;

                //int rowLimit = Mathf.Min(r1 + MAX_CELLS, ROW);
                for (int r2 = r1; r2 < currentRow; r2++)
                    for (int c2 = c1; c2 < currentCol; c2++)
                    {
                        // 면적 기반 최적화
                        int area = GetActiveCountFast(r1, c1, r2, c2);
                        if (area < 2) continue;
                        if (area > MAX_CELLS) break;

                        int currentSum = GetRectSumFast(r1, c1, r2, c2);

                        if (currentSum == MAX_NUM)
                        {// 10을 찾으면 해당 범위 내의 모든 글자 인덱스를 계산해서 반환
                            //Debug.Log($"GetRectSumFast {currentSum} / ({r1}, {c1}, {r2}, {c2})");
                            ChageColorRect(r1, c1, r2, c2);
                            return;
                        }else if (currentSum > MAX_NUM) break;
                    }
            }
    }
    
    void ChageColorRect(int r1, int c1, int r2, int c2)
    {// 사각형 좌표를 TMP 글자 인덱스로 변환하는 보조 함수
        for (int r = r1; r <= r2; r++)
            for (int c = c1; c <= c2; c++)
                // 지워진 숫자가 아닐 때만 인덱스 추가
                if (grid[r, c] != 0)
                {// TMP 인덱스 공식: (행 * 한줄당글자수) + (열 * 2 [숫자+공백])
                    int index = PosToIndex(r, c);
                    SetCharColor(index, highlightColor);
                }
        mainText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    IEnumerator RemoveAllRutine()
    {
        SetPause(true);
        //Debug.Log("RemoveAllRutine");
        for (int r = 0; r < currentRow; r++)
            for (int c = 0; c < currentCol; c++)
            {
                if (grid[r, c] == 0) continue;
                int index = PosToIndex(r, c);
                RemoveChar(index, true);
                mainText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                yield return YieldCache.WaitForSeconds(.05f);
            }
        SetPause(false);
    }

    public void OnGameEnd() => StartCoroutine(RemoveAllRutine());

    IEnumerator ShuffleRutine()
    {
        //yield return null;
        //Debug.Log($"{mainText.textInfo.characterCount} / {ROW} /{COL}");
        if (mainText.textInfo.characterCount > 0 && !IsClear)
        {
            for (int r = 0; r < currentRow; r++)
                for (int c = 0; c < currentCol; c++)
                {
                    if (grid[r, c] == 0) continue;
                    int index = PosToIndex(r, c);
                    //Debug.Log(index);
                    SetCharColor(index, shuffleColor);
                }
            mainText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
        SetPause(true);
        GameManager.I.isSuffle = true;
        yield return YieldCache.WaitForSeconds(.5f);

        for (int i = 0; i < 10; i++)
        {
            //GenerateGrid();
            UpdateTextDisplay(true);
            yield return YieldCache.WaitForSeconds(.04f);
        }
        GameManager.I.isSuffle = false;
        SetPause(false);
        if (!isCustom)
            GenerateGrid();
        UpdateTextDisplay();
        OnChangeNumber();
        UpdatePrefixSum();
        isCustom = false;
        //yield return YieldCache.WaitForSeconds(.5f);


    }

    void PlaySound() => audio?.PlayOneShot(removeCharSound);
    public void SetPause(bool _pause) => GameManager.Instance.SetPause(isPause = _pause);
    // 10개 숫자 + 10개 공백 + 1개 줄바꿈
    Vector2Int IndexToPos(int i) => new(i / CHARS_PER_LINE, (i % CHARS_PER_LINE) >> 1);
    int PosToIndex(int r,int c) => (r * CHARS_PER_LINE) + (c << 1);
    int GetRectSumFast(int r1, int c1, int r2, int c2) =>
        // prefixSum이 1-based 테이블이라고 가정할 때 (r+1, c+1 지점)
        sumTable[r2 + 1, c2 + 1] -
        sumTable[r1, c2 + 1] -
        sumTable[r2 + 1, c1] +
        sumTable[r1, c1];

    int GetActiveCountFast(int r1, int c1, int r2, int c2) =>
        countTable[r2 + 1, c2 + 1] -
        countTable[r1, c2 + 1] -
        countTable[r2 + 1, c1] +
        countTable[r1, c1];
    bool IsClear => GetActiveCountFast(0, 0, currentRow - 1, currentCol - 1) == 0;

    int CHARS_PER_LINE => (currentCol << 1) + 1;
}

