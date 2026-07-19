using System.Collections;
using NUnit.Framework;
using Telerobot.Game.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class HudAndRadioPlayModeTests : RuntimeSceneTestBase
    {
        private static readonly string[] Keys =
        {
            "radio.game_start", "radio.phase1", "radio.phase2", "radio.phase3", "radio.battery_warning",
            "radio.base_danger", "radio.phase_clear", "radio.victory"
        };

        private static readonly string[] Values =
        {
            "텔레 로봇팀, 출격하라.", "감염체 접근. 북쪽 도로 방어 준비.", "동쪽 골목에서 추가 접근 신호 감지.",
            "남쪽 터널 개방. 메디컬 로봇 투입.", "해태 1호, 배터리 위험.", "거점 방어선 붕괴 임박.",
            "위협 제거. 재정비 단계 진입.", "거점 생존 확인. 작전 성공."
        };

        [UnityTest]
        public IEnumerator EightRadioStringsRemainByteExact()
        {
            for (var index = 0; index < Keys.Length; index++)
                Assert.That(Game.Catalog.strings.Get(Keys[index]), Is.EqualTo(Values[index]));
            var hud = Object.FindFirstObjectByType<CombatHud>();
            Assert.That(hud, Is.Not.Null);
            Assert.That(hud.GetComponent<AudioSource>(), Is.Not.Null, "Radio captions must include placeholder audio feedback.");
            yield return null;
        }
    }
}
