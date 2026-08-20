# Game Data 후속 작업 체크리스트

- [ ] `SkillInfo`에 `StringKey` 열을 추가하고 현지화 테이블로 스킬 설명을 전환한다.
- [ ] `Sheet1`의 효과 수치와 문자열 성장식을 `Base` / `PerLevel` 열로 정규화한다.
- [ ] 성장식의 `x`를 `Player.Level`로 계산하는 공통 스킬 수치 계산기를 추가한다.
- [ ] `CastSkill.Cast(skillId)`에 11개 실제 효과를 데이터 기반으로 연결한다.
- [x] `StatUpgrade` 카테고리에 최대 체력, 피해 감소, 회복 효율 강화 데이터를 추가하고 플레이어 적용 API를 연결한다.
- [ ] 각 스킬의 `ImagePath`를 채우고 카드 기본 이미지를 실제 아이콘으로 교체한다.

현재 `SkillInfo.Description`은 카드 UI에 직접 표시하며, 원본 `Sheet1`의 효과 수치와 성장식은 그대로 보존한다.

`SkillInfo.Grade`는 원본 스킬 메타데이터 보존용이며 현재 초 단위 발동 판정에는 사용하지 않는다.
능력치 강화 수치와 누적 상한은 `StatValue` / `StatCap`에서 조정한다. `StatCap` 0은 무제한을 뜻한다.
