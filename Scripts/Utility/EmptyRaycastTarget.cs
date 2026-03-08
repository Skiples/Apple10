using UnityEngine.UI;// 이 스크립트를 빈 오브젝트에 넣으면 클릭 가능한 투명 영역이 됩니다.
public class EmptyRaycastTarget : MaskableGraphic
{// 메쉬를 생성하지 않음 (렌더링 0)
    protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();
}