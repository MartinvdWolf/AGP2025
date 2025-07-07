using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTypeConfiguration : MonoBehaviour
{
    public enum planetTypes { Desert, Ice, Earth, Gas, Volcano, Sun };

    public Material desertMat;
    public Material iceMat;
    public Material earthMat;
    public Material gasMat;
    public Material volMat;
    public Material sunMat;

    public float ringRadiusOffset = 100f;
    public float ringWidthOffset = 50f;

    public Material SetType(GameObject planet, bool isSun = false)
    {
        planetTypes curType;
        curType = (planetTypes)Random.Range(0, 5);
        Material curMat;
        if (isSun)
            curType = planetTypes.Sun;

        switch (curType)
        {
            case planetTypes.Desert:
                curMat = desertMat;
                planet.tag = "desert";
                break;
            case planetTypes.Ice:
                curMat = iceMat;
                planet.tag = "ice";
                break;
            case planetTypes.Earth:
                curMat = earthMat;
                planet.tag = "earth";
                break;
            case planetTypes.Gas:
                curMat = gasMat;
                planet.tag = "gas";
                break;
            case planetTypes.Volcano:
                curMat = volMat;
                planet.tag = "volcano";
                break;
            case planetTypes.Sun:
                    curMat = sunMat;
                planet.tag = "sun";
                break;
            default:
                curMat = earthMat;
                planet.tag = "earth";
                break;
        }

        return curMat;
    }
}
