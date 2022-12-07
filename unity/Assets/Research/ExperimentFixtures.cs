using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorldAsSupport.Research {
    public enum Variant {
        wp_ARROWS,
        wp_EMBODIED,
        wp_CONTROL,
        exp_WOW,
        exp_WAS
    }
    public class ExperimentFixtures : MonoBehaviour {
        public List<GameObject> Prefabs;
        public Variant RuntimeVariant;
        
        private Dictionary<Variant, Func<object>> Fixtures;
        private string SceneName;

        // common components
        private WaypointProvider wp;
        private ExperimentManager experiment;

        // singleton instance
        public static ExperimentFixtures current;

        public void InitFixtures() {
            // common components
            wp = transform.GetComponent<WaypointProvider>();
            experiment = transform.GetComponent<ExperimentManager>();
            Fixtures = new Dictionary<Variant, Func<object>>() {
                { Variant.wp_ARROWS, () => {
                    wp_common();
                    wp.GuidePrefabs[0] = Prefabs.Find(go => go.name == "Arrow Guide").GetComponent<WaypointGuide>();
                    experiment.SurveyUrl += "&q=QWP&u_group=1";
                    return null;
                }},
                { Variant.wp_EMBODIED, () => {
                    wp_common();
                    wp.GuidePrefabs[0] = Prefabs.Find(go => go.name == "Human Guide").GetComponent<WaypointGuide>();
                    experiment.SurveyUrl += "&q=QWP&u_group=2";
                    return null;
                }},
                { Variant.wp_CONTROL, () => {
                    wp_common();
                    wp.enabled = false;
                    experiment.SurveyUrl += "&q=QWP&u_group=3";
                    return null;
                }},
                { Variant.exp_WAS, () => {
                    exp_common();                 
                    experiment.SurveyUrl += "&q=QEX&u_group=1";
                    return null;
                }},
                { Variant.exp_WOW, () => {
                    exp_common();
                    transform.GetComponent<ExperiencesManager>().isWindow_on_the_World = true;
                    experiment.SurveyUrl += "&q=QEX&u_group=2";
                    return null;
                }}
            };
        } 

        private void wp_common() {
            wp.enabled = true;
            SceneName = "BarcinoMap_waypoints";
            transform.GetComponent<ExperiencesManager>().isWindow_on_the_World = false;
            experiment.IntroModalPrefab = Prefabs.Find(go => go.name == "ExperimentIntroModal");
            experiment.ProjectId = "upf-was-2020-exp01";
            experiment.SurveyUrl = "https://www.soscisurvey.de/upf-was-2020/?r={SID}";
        }

        private void exp_common() {
            wp.enabled = false;
            SceneName = "BarcinoMap_Exp";
            transform.GetComponent<ExperiencesManager>().isWindow_on_the_World = false;
            experiment.IntroModalPrefab = Prefabs.Find(go => go.name == "ExperimentIntroModal");
            experiment.ProjectId = "upf-ar-barcino";
            experiment.SurveyUrl = "https://www.soscisurvey.de/upf-was-2020/?r={SID}";
        }

        void Awake() {
            // assign singleton
            ExperimentFixtures.current = this;
        }

        public void ApplyFixtures() {
            InitFixtures();
            Fixtures[RuntimeVariant]();
        #if !UNITY_EDITOR && !UNITY_IOS
            SceneManager.LoadScene(SceneName, LoadSceneMode.Additive);
        #endif
        }
        
    }
}